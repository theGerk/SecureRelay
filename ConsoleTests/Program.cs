
using System.Net;
using System.Net.Sockets;
using Gerk.AsyncThen;

TcpListener tcpListener = new TcpListener(IPAddress.Any, 4444);
tcpListener.Start();
var aliceTask = tcpListener.AcceptTcpClientAsync();
var bobTask = tcpListener.AcceptTcpClientAsync();

Task.WhenAll(aliceTask, bobTask).Then(x =>
{
	SecureRelay.SecureRelay.Relay(x[0].GetStream(), x[1].GetStream());
});


object lck = new();


var aliceThread = new Thread(() =>
{
	var clinet = new TcpClient();
	clinet.Connect(IPAddress.Loopback, 4444);
	using var strm = clinet.GetStream();
	using var writer = new BinaryWriter(strm);
	writer.Write("This is alice, is bob there?");
	using var reader = new BinaryReader(strm);
	lock (lck)
		Console.WriteLine("to ALICE from BOB: " + reader.ReadString());

});


var bobThread = new Thread(() =>
{
	var client = new TcpClient();
	client.Connect(IPAddress.Loopback, 4444);
	using var stream = client.GetStream();
	using var reader = new BinaryReader(stream);
	lock (lck)
		Console.WriteLine("to BOB from ALICE: " + reader.ReadString());
	using var writer = new BinaryWriter(stream);
	writer.Write("Listen here you fuck, I told you not to talk to me any more!");
});


aliceThread.Start();
bobThread.Start();

aliceThread.Join();
bobThread.Join();