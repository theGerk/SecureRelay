using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using SecureRelay;

namespace SecureClient
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var key = new RSACryptoServiceProvider();
			SecureRelayClient client = SecureRelayClient.Start(new IPEndPoint(IPAddress.Loopback, 9092), new IPEndPoint(IPAddress.Loopback, 9090), new IPEndPoint(IPAddress.Loopback, 9091), key, null);
			System.Threading.Thread.Sleep(-1);

		}
	}
}