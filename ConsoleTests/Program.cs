
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Gerk.AsyncThen;
using Gerk.Crypto.EncryptedTransfer;
using SecureRelay;


RSACryptoServiceProvider clientKey = new RSACryptoServiceProvider();
RSACryptoServiceProvider serverKey = new RSACryptoServiceProvider();

TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 2221);
tcpListener.Start();

SecureRelayServer secureRelayServer = SecureRelayServer.Start(new IPEndPoint(IPAddress.Loopback, 2222), serverKey, new[] { new rsahaspublickeysha(clientKey) });
SecureRelayClient secureRelayClient = SecureRelayClient.Start(
	new IPEndPoint(IPAddress.Loopback, 2223),
	new IPEndPoint(IPAddress.Loopback, 2222),
	new IPEndPoint(IPAddress.Loopback, 2221),
	clientKey,
	new[] { new rsahaspublickeysha(serverKey) }
);

TcpClient tcpClient = new();
tcpClient.Connect(IPAddress.Loopback, 2223);
var aliceStream = tcpClient.GetStream();


object lck = new();


var aliceThread = new Thread(() =>
{
	using var strm = aliceStream;
	using var writer = new BinaryWriter(strm);
	writer.Write("This is alice, is bob there?");
	using var reader = new BinaryReader(strm);
	var text = reader.ReadString();
	lock (lck)
		Console.WriteLine("to ALICE from BOB: " + text);

});


var bobThread = new Thread(() =>
{
	using var stream = tcpListener.AcceptTcpClient().GetStream();
	using var reader = new BinaryReader(stream);
	var text = reader.ReadString();
	lock (lck)
		Console.WriteLine("to BOB from ALICE: " + text);
	using var writer = new BinaryWriter(stream);
	writer.Write("Listen here you fuck, I told you not to talk to me any more!");
});


aliceThread.Start();
bobThread.Start();

aliceThread.Join();
bobThread.Join();


public class rsahaspublickeysha : IHasPublicKeySha, IDisposable
{
	public RSACryptoServiceProvider RSA { get; set; }

	public rsahaspublickeysha(RSACryptoServiceProvider rSACryptoServiceProvider)
	{
		RSA = rSACryptoServiceProvider;
	}

	public byte[] GetPublicKeySha() => SHA256.HashData(RSA.ExportCspBlob(false));

	public void Dispose() => RSA.Dispose();
}
