using Gerk.Crypto.EncryptedTransfer;
using SecureRelay;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;



//RSACryptoServiceProvider clientKey = new RSACryptoServiceProvider();
//RSACryptoServiceProvider serverKey = new RSACryptoServiceProvider();

//TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 2221);
//tcpListener.Start();

//SecureRelayServer secureRelayServer = SecureRelayServer.Start(new IPEndPoint(IPAddress.Loopback, 2222), serverKey, new[] { new rsahaspublickeysha(clientKey) });
//SecureRelayClient secureRelayClient = SecureRelayClient.Start(
//	new IPEndPoint(IPAddress.Loopback, 2223),
//	new IPEndPoint(IPAddress.Loopback, 2222),
//	new IPEndPoint(IPAddress.Loopback, 2221),
//	clientKey,
//	new[] { new rsahaspublickeysha(serverKey) }
//);

//TcpClient tcpClient = new();
//tcpClient.Connect(IPAddress.Loopback, 2223);
//var aliceStream = tcpClient.GetStream();


//object lck = new();



public static class Program
{
	public static void Main(string[] args)
	{
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		SecureRelayServer server = SecureRelayServer.Start(new System.Net.IPEndPoint(IPAddress.Loopback, 9090), rSACryptoServiceProvider, null);
		System.Threading.Thread.Sleep(-1);
	}
}