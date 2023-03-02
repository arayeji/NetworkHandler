using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkHandlerUnitTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            NetworkHandler.UDPNetwork server = new NetworkHandler.UDPNetwork(new System.Net.IPEndPoint(IPAddress.Any, 1234), NetworkHandler.NetworkBase.Modes.Server, System.Net.Sockets.AddressFamily.InterNetwork);
            server.OnPacketReceived += Server_OnPacketReceived;
            server.Start();

            NetworkHandler.UDPNetwork client = new NetworkHandler.UDPNetwork(new System.Net.IPEndPoint(IPAddress.Any, 0), NetworkHandler.NetworkBase.Modes.Client, System.Net.Sockets.AddressFamily.InterNetwork);   ;
            client.OnPacketReceived += Client_OnPacketReceived;
            client.Start();
            byte[] data = Encoding.ASCII.GetBytes("Echo");
            client.Send(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234), data, data.Length, true);
            Console.ReadLine();
            //  net.mode
            //  net.Send()

        }

        private static void Client_OnPacketReceived(object sender, NetworkHandler.NetworkEventHandler e)
        {
            e.Network.Send(e.EndPoint, e.Packet, e.Packet.Length, false);
        }

        private static void Server_OnPacketReceived(object sender, NetworkHandler.NetworkEventHandler e)
        {
          Console.WriteLine(Encoding.ASCII.GetString(e.Packet));
        }

        
    }
}