using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Gerk.Crypto.EncryptedTransfer;
using Gerk.BinaryExtension;

namespace SecureRelay
{
	public static class SecureRelay
	{
		// Work in progress. Need to handle errors still and test.
		public static async Task FromSecureToInsecure(Stream secure, RSACryptoServiceProvider localPrivateKey, IEnumerable<Identity> identities)
		{
			using var tunnel = Tunnel.Create(secure, localPrivateKey, identities, out Identity user, out TunnelCreationError error, out string errorMessege);
			if (error != TunnelCreationError.NoError)
				return;

			using var reader = new BinaryReader(secure);
			if (!IPEndPoint.TryParse(reader.ReadString(), out var endpoint))
				return;

			using var forward = new TcpClient(endpoint).GetStream();
			await Relay(secure, forward);
		}

		public static async Task FromInsecureToSecure(Stream insecure, IPEndPoint target, RSACryptoServiceProvider localPrivateKey, IEnumerable<Identity> identities)
		{
			using var forwardedStream = new TcpClient(target).GetStream();
			using var secure = Tunnel.Create(forwardedStream, localPrivateKey, identities, out Identity user, out TunnelCreationError error, out string errorMessege);
			await Relay(insecure, secure);
		}

		private static Task Relay(Stream alice, Stream bob)
		{
			Task aliceToBob = bob.CopyToAsync(alice);
			Task bobToAlice = alice.CopyToAsync(bob);
			return Task.WhenAll(aliceToBob, bobToAlice);
		}
	}
}