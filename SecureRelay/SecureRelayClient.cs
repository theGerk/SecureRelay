using System;
using System.Collections.Concurrent;
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
	public class SecureRelayClient : IAsyncDisposable
	{
		private volatile bool active = true;
		private uint count = 0;
		private TaskCompletionSource activeTaskCompletionSource = new TaskCompletionSource();
		private readonly object lockobj = new();

		private SecureRelayClient() { }

		public static SecureRelayClient Start(IPEndPoint listeningIp, IPEndPoint targetIp, IPEndPoint finalIpTarget, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validTargets)
		{
			var output = new SecureRelayClient();
			output.Run(listeningIp, targetIp, finalIpTarget, myKey, validTargets);
			return output;
		}

		public async ValueTask DisposeAsync()
		{
			lock (lockobj)
			{
				active = false;
			}
			await activeTaskCompletionSource.Task;
		}

		private async void Run(IPEndPoint listeningIp, IPEndPoint secureRemoteServer, IPEndPoint finalIpTarget, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validTargets)
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
				SecureRelay.FromInsecureToSecure(localInsecureConnection.GetStream(), secureRemoteServer, finalIpTarget, myKey, validTargets)
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
