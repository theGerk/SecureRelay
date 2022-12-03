using System.Security.Cryptography;
using Gerk.Crypto.EncryptedTransfer;
using SecureRelay;

public static class Program
{
	public static void Main(string[] args)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		SecureRelayServer server = SecureRelayServer.Start(new System.Net.IPEndPoint(IPAddress.Loopback, 9090), rSACryptoServiceProvider, null);
		System.Threading.Thread.Sleep(-1);
	}
}