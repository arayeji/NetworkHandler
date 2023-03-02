using System.ComponentModel;
using System.Net;
using System.Text;

namespace NetworkHandlerTest
{
    [TestClass]
    public class NetworkHandlerTests
    {
        [TestMethod]
        public void TestClientServer()
        {
            string Message = "Echo";
            string Response = "";
            int Timeout = 300;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(Timeout);
            NetworkHandler.UDPNetwork server = new NetworkHandler.UDPNetwork(new System.Net.IPEndPoint(IPAddress.Any, 1234), NetworkHandler.NetworkBase.Modes.Server, System.Net.Sockets.AddressFamily.InterNetwork);
            server.OnPacketReceived += delegate (object sender, NetworkHandler.NetworkEventHandler e)
            {
                Response = Encoding.ASCII.GetString(e.Packet,0,e.TransferedBytes);
                cts.Cancel();
            };

            server.Start();

            NetworkHandler.UDPNetwork client = new NetworkHandler.UDPNetwork(new System.Net.IPEndPoint(IPAddress.Any, 0), NetworkHandler.NetworkBase.Modes.Client, System.Net.Sockets.AddressFamily.InterNetwork); ;
            client.OnPacketReceived += delegate (object sender, NetworkHandler.NetworkEventHandler e)
            {
                e.Network.Send(e.EndPoint, e.Packet, e.Packet.Length, false);
            };

            client.Start();
            byte[] data = Encoding.ASCII.GetBytes("Echo");
            client.Send(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), data, data.Length, true);
            while(!cts.IsCancellationRequested)
            {
                Thread.Sleep(1);
            }
            Assert.AreEqual(Message, Response);
        }
    }
}