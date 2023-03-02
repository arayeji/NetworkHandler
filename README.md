Simple Socket Client And Server Usage:

Listening:
```c#
 NetworkHandler.UDPNetwork server = new NetworkHandler.UDPNetwork(new System.Net.IPEndPoint(IPAddress.Any, 1234), NetworkHandler.NetworkBase.Modes.Server, System.Net.Sockets.AddressFamily.InterNetwork);
 server.OnPacketReceived += Server_OnPacketReceived;
 server.Start();
```

Receiving Data:
```c#
private static void Server_OnPacketReceived(object sender, NetworkHandler.NetworkEventHandler e)
{
  Console.WriteLine(Encoding.ASCII.GetString(e.Packet));
}
```
