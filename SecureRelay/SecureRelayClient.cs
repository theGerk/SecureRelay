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
	public class SecureRelayClient
	{
		/// <summary>
		/// State of the connection. Starts at waiting, then starts relaying until the connection is closed.
		/// </summary>
		public enum State { WaitingForConnection, Relaying, Closed };
		public State CurrentState { get; private set; } = State.WaitingForConnection;

		private readonly TaskCompletionSource WhenConnectedTaskSource = new();
		private readonly TaskCompletionSource WhenCompletedTaskSource = new();

		public Task WhenConnected => WhenConnectedTaskSource.Task;
		public Task WhenCompleted => WhenCompletedTaskSource.Task;

		private SecureRelayClient() { }

		public static SecureRelayClient Start(IPEndPoint listeningIp, IPEndPoint targetIp, IPEndPoint finalIpTarget, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validTargets)
		{
			var output = new SecureRelayClient();
			output.Run(listeningIp, targetIp, finalIpTarget, myKey, validTargets);
			return output;
		}

		private async void Run(IPEndPoint listeningIp, IPEndPoint secureRemoteServer, IPEndPoint finalIpTarget, RSACryptoServiceProvider myKey, IEnumerable<IHasPublicKeySha> validTargets)
		{
			var tcpListener = new TcpListener(listeningIp);
			tcpListener.Start();

			var localInsecureConnection = await tcpListener.AcceptTcpClientAsync();
			tcpListener.Stop();
			CurrentState = State.Relaying;
			WhenConnectedTaskSource.SetResult();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			SecureRelay.FromInsecureToSecure(localInsecureConnection.GetStream(), secureRemoteServer, finalIpTarget, myKey, validTargets)
				.Then(() => {
					CurrentState = State.Closed;
					WhenCompletedTaskSource.SetResult();
				});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}
	}
}
