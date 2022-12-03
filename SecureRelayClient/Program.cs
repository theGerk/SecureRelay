using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using SecureRelay;
using Gerk.Crypto.EncryptedTransfer;

namespace SecureClient
{
	public static class Program
	{
		/// <summary>
		/// Starts a server 
		/// </summary>
		/// <param name="listen"></param>
		/// <param name="target"></param>
		public static async Task Main(string[] args)
		{
			if(args.Length != 3)
			{
				Console.WriteLine("Give arguments of:\nListening Endpoint\nTarget Endpoint\nFinal Endpoint\nServer Public Key");
			}

			var rsa = await RsaUtil.GetOrGeneratePrivateKey("privateKey");
			Console.WriteLine("Your public key is:");
			Console.WriteLine(RsaUtil.GetShaString(rsa));

			SecureRelayClient.Start(IPEndPoint.Parse(args[0]), IPEndPoint.Parse(args[1]), IPEndPoint.Parse(args[2]), rsa, new IHasPublicKeySha[] { new RsaKey(args[3]) });
		}
	}
}