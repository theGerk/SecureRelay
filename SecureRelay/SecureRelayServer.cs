using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Gerk.AsyncThen;
using Gerk.Crypto.EncryptedTransfer;

namespace SecureRelay
{
	public class SecureRelayServer : IAsyncDisposable
	{
		private volatile bool active = true;
		private uint count = 0;
		private readonly TaskCompletionSource activeTaskCompletionSource = new();
		private readonly object lockobj = new();

		private SecureRelayServer() { }

		public static SecureRelayServer Start(IPEndPoint listeningIp, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validSources, Func<IHasPublicKeySha, IPEndPoint, bool> hasPermission, Action<string> log = null, Action<TunnelCreationError, string> handleError = null)
		{
			var output = new SecureRelayServer();
			output.Run(listeningIp, myKey, validSources, hasPermission, log ?? (_ => { }), handleError);
			return output;
		}

		public async ValueTask DisposeAsync()
		{
			if (active)
				lock (lockobj)
					active = false;
			await activeTaskCompletionSource.Task;
		}

		private async void Run(IPEndPoint listeningIp, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validSources, Func<IHasPublicKeySha, IPEndPoint, bool> hasPermission, Action<string> log, Action<TunnelCreationError, string> handleError)
		{
			var tcpListener = new TcpListener(listeningIp);
			tcpListener.Start();

			while (active)
			{
				var localInsecureConnection = await tcpListener.AcceptTcpClientAsync();
				lock (lockobj)
				{
					if (!active)
						return;
					++count;
				}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				SecureRelay.FromSecureToInsecure(localInsecureConnection.GetStream(), myKey, validSources, hasPermission, log, handleError)
					.Then(() =>
					{
						uint count;
						bool active;
						lock (lockobj)
						{
							count = --this.count;
							active = this.active;
						}

						if (count == 0 && !active)
							activeTaskCompletionSource.SetResult();
					});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}

		}
	}
}
