using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Gerk.Crypto.EncryptedTransfer;
using Gerk.BinaryExtension;

namespace SecureRelay
{
	public static class SecureRelay
	{
		public static async Task FromSecureToInsecure(Stream encryptedInputStream, RSACryptoServiceProvider localPrivateKey, IEnumerable<IHasPublicKeySha> identities)
		{
			using var rawTunnel = Tunnel.Create(encryptedInputStream, localPrivateKey, identities, out IHasPublicKeySha user, out TunnelCreationError error, out string errorMessege);
			using var safeTunnel = new SafeTunnel(rawTunnel);
			if (error != TunnelCreationError.NoError)
				return;

			using var reader = new BinaryReader(rawTunnel);
			if (!IPEndPoint.TryParse(reader.ReadString(), out var endpoint))
				return;
			rawTunnel.FlushReader();

			using var f = new TcpClient();
			await f.ConnectAsync(endpoint);
			using var forward = f.GetStream();
			await Relay(safeTunnel, forward);
		}

		public static async Task FromInsecureToSecure(Stream clearTextInputStream, IPEndPoint target, IPEndPoint finalTarget, RSACryptoServiceProvider localPrivateKey, IEnumerable<IHasPublicKeySha> identities)
		{
			using var f = new TcpClient();
			f.Connect(target);
			using var forwardedStream = f.GetStream();
			using var rawTunnel = Tunnel.Create(forwardedStream, localPrivateKey, identities, out IHasPublicKeySha user, out TunnelCreationError error, out string errorMessege);
			using var safeTunnel = new SafeTunnel(rawTunnel);
			if (error != TunnelCreationError.NoError)
				return;

			using var writer = new BinaryWriter(rawTunnel);
			writer.Write(finalTarget.ToString());
			rawTunnel.FlushWriter();

			await Relay(clearTextInputStream, safeTunnel);
		}

		private static Task Relay(Stream alice, Stream bob)
		{
			Task run(Stream from, Stream to)
			{
				return Task.Run(() =>
				{
					byte[] buff = new byte[4096];
					while (true)
					{
						int len = from.Read(buff, 0, buff.Length);
						if (len <= 0)
							break;
						to.Write(buff, 0, len);
					}
				});
			}

			Task aliceToBob = run(alice, bob);
			Task bobToAlice = run(bob, alice);
			return Task.WhenAll(aliceToBob, bobToAlice);
		}
	}
}