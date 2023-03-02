//using PacketDotNet;
//using SharpPcap;
//using System.Collections.Concurrent;
//using System.ComponentModel;
//using System.Net;
//using System.Net.Sockets;
//using System.Reflection;

//namespace NetworkHandler
//{

//    public class NetworkHandler : EventArgs
//    {
//        public byte[] Packet;
//        public int TransferedBytes;
//        public IPEndPoint EndPoint;
//        public Network Network;
//    }
//    public class NetworkHandlerError : EventArgs
//    {
//        public string Error;
//        public Exception exception;
//    }
//    public class NetworkPool
//    {
//        List<Network> networks = new List<Network>();
//        public NetworkPool(int PoolCount)
//        {
//            for (int x = 0; x < PoolCount; x++)
//            {
//                Network net = new Network(new IPEndPoint(IPAddress.Any, 0), Network.ConnectionType.UDP, Network.Engine.Native, Network.Modes.Client);
//                net.State = Network.States.Ready;
//                net.InPool = true;
//                net.BindAndReceive();
//                networks.Add(net);
//            }
//        }

//        public void ReleasePool()
//        {
//            networks.ForEach(x => x.Stop());
//        }

//        public void CloseAllSockets()
//        {
//            networks.ForEach(x => x.Stop());
//        }

//        public void Release(Network network, IPEndPoint Endpoint)
//        {
//            network.Stop();
//        }


//    }


//    public class Network
//    {
//        public void RemoveHandlers()
//        {


//            foreach (var x in this.OnPacketReceived.GetInvocationList())
//            {
//                this.OnPacketReceived -= (OnPacketReceivedHandler)x;
//            }

//        }

//        bool Started = false;
//        public delegate void OnPacketReceivedHandler(object sender, NetworkHandler e);
//        public event OnPacketReceivedHandler OnPacketReceived;

//        public delegate void OnErrorHandler(object sender, NetworkHandlerError e);

//        public event OnErrorHandler OnError;


//        public enum ConnectionType
//        {
//            UDP, TCP, SCTP
//        }

//        public enum Engine
//        {
//            Native, PCAP
//        }

//        public enum Modes
//        {
//            Client, Server
//        }

//        public enum ClientModes
//        {
//            Reqular, Pool
//        }

//        public int QueueCount = 1000;
//        AddressFamily _AddressFamily = AddressFamily.InterNetwork;

//        public DateTime LastUsed = DateTime.Now.AddMinutes(-1);
//        public States State;
//        public enum States
//        {
//            NotReady, Ready, Reserved
//        }


//        Socket NetworkSocket = null;
//        Engine _engine = Engine.Native;
//        IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);
//        ConnectionType _connectionType = ConnectionType.TCP;
//        Modes _mode = Modes.Server;


//        CaptureDeviceList devlist = CaptureDeviceList.Instance;
//        List<ICaptureDevice> Devices = new List<ICaptureDevice>();

//        public void Listening()
//        {
//            List<string> list = new List<string>();


//            for (int i = 0; i < devlist.Count; i++)
//            {

//                try
//                {
//                    SharpPcap.ICaptureDevice devcap = devlist[i];

//                    Console.WriteLine(devcap.Name);
//                    devcap.Open(DeviceModes.Promiscuous);
//                    {
//                        Devices.Add(devcap);
//                        devcap.OnPacketArrival += EtherPacket_OnPacketArrival;


//                        if (!_endPoint.Address.Equals(IPAddress.Parse("0.0.0.0")))
//                            devcap.Filter = _connectionType.ToString().ToLower() + " && host " + _endPoint.Address.ToString() + " && port " + _endPoint.Port;
//                        else
//                            devcap.Filter = _connectionType.ToString().ToLower() + "  && port " + _endPoint.Port;


//                        devcap.StartCapture();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    if (OnError != null)
//                        OnError(this, new NetworkHandlerError { Error = "Listening: " + _connectionType.ToString() + " && host " + _endPoint.Address.ToString() + " && port " + _endPoint.Port + "\r\n" + ex.ToString(), exception = ex });
//                }
//            }
//        }

//        void EtherPacket_OnPacketArrival(object sender, PacketCapture e)
//        {
//            try
//            {
//                OnPacketReceived(sender, new NetworkHandler { Packet = e.GetPacket().Data });



//            }
//            catch (Exception ex)
//            {
//                if (OnError != null)
//                    OnError(this, new NetworkHandlerError { exception = ex, Error = "OnPacketReceived: " + ex.Message });
//            }
//        }


//        public Network(IPEndPoint endPoint, ConnectionType connectionType, Engine engine, Modes mode)
//        {

//            _engine = engine;
//            _connectionType = connectionType;
//            _endPoint = endPoint;
//            _mode = mode;

//            switch (connectionType)
//            {
//                case ConnectionType.UDP:
//                    {
//                        switch (engine)
//                        {
//                            case Engine.Native:
//                                {
//                                    NetworkSocket = new Socket(_AddressFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
//                                }
//                                break;

//                        }

//                    }
//                    break;
//                case ConnectionType.TCP:
//                    {
//                        switch (engine)
//                        {
//                            case Engine.Native:
//                                {
//                                    NetworkSocket = new Socket(_AddressFamily, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
//                                }
//                                break;
//                        }
//                    }
//                    break;
//                case ConnectionType.SCTP:
//                    {
//                        switch (engine)
//                        {
//                            case Engine.Native:
//                                {
//                                    NetworkSocket = new Socket(_AddressFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

//                                }
//                                break;
//                        }
//                    }
//                    break;
//            }
//        }

//        public void Send(IPEndPoint endPoint, byte[] Data, int Length, bool WaitForResponse)
//        {

//            if (Started)
//            {
//                SendAsync(endPoint, Data, Length, WaitForResponse);
//            }
//        }
//        int LoopDetector = 0;
//        void SendAsync(IPEndPoint endPoint, byte[] Data, int Length, bool WaitForResponse)
//        {
//            if (Started)
//            {
//                SocketAsyncEventArgs rServerAccEvent = new SocketAsyncEventArgs();
//                rServerAccEvent.RemoteEndPoint = endPoint;
//                Socket So = NetworkSocket;


//                rServerAccEvent.Completed += delegate (object? sender, SocketAsyncEventArgs e)
//                {
//                    if (WaitForResponse && Started)
//                        ReceiveAsync(endPoint, So, true);

//                };
//                rServerAccEvent.SetBuffer(Data, 0, Length);

//                bool ok = So.SendToAsync(rServerAccEvent);
//                if (!ok)
//                {

//                    if (WaitForResponse && Started)
//                        ReceiveAsync(endPoint, So, true);

//                }
//                rServerAccEvent.Dispose();
//            }
//        }

//        bool IsReady = true;

//        public void RServerAccEvent_Completed(object? sender, SocketAsyncEventArgs e)
//        {
//            IsReady = true;
//            try
//            {

//                if (Started)
//                {

//                    if (e.BytesTransferred > 0)
//                    {
//                        OnPacketReceived(sender, new NetworkHandler { Network = this, Packet = e.Buffer, TransferedBytes = e.BytesTransferred, EndPoint = (IPEndPoint)e.RemoteEndPoint });

//                        e.Dispose();
//                        if (!(bool)e.UserToken)
//                            ReceiveAsync(new IPEndPoint(IPAddress.Any, 0), (Socket)(sender), (bool)e.UserToken);


//                    }
//                }
//                else
//                    e.Dispose();

//            }
//            catch
//            {

//            }

//        }
//        void ReceiveAsync(IPEndPoint endPoint, Socket So, bool OneTime)
//        {

//            while (Started)
//            {

//                if (So != null)
//                {
//                    try
//                    {


//                        SocketAsyncEventArgs rServerAccEvent = new SocketAsyncEventArgs();
//                        byte[] buffer = new byte[1600];

//                        rServerAccEvent.UserToken = OneTime;
//                        rServerAccEvent.RemoteEndPoint = endPoint;
//                        rServerAccEvent.Completed += RServerAccEvent_Completed;
//                        rServerAccEvent.SetBuffer(buffer, 0, 1600);

//                        bool ok = So.ReceiveFromAsync(rServerAccEvent);
//                        if (!ok)
//                        {

//                            IsReady = true;

//                            try
//                            {

//                                if (Started)
//                                {

//                                    if (rServerAccEvent.BytesTransferred > 0)
//                                    {
//                                        OnPacketReceived(So, new NetworkHandler { Network = this, Packet = rServerAccEvent.Buffer, TransferedBytes = rServerAccEvent.BytesTransferred, EndPoint = (IPEndPoint)rServerAccEvent.RemoteEndPoint });

//                                        rServerAccEvent.Dispose();

//                                    }
//                                }
//                                else
//                                    rServerAccEvent.Dispose();

//                            }
//                            catch
//                            {

//                            }


//                        }
//                        else
//                            break;

//                        if (!OneTime)
//                            IsReady = false;

//                    }
//                    catch (System.ObjectDisposedException)
//                    {

//                    }
//                }

//            }
//        }

//        public bool Stop()
//        {
//            if (Started)
//            {
//                Started = false;
//                RemoveHandlers();

//                if (!InPool)
//                {
//                    State = Network.States.NotReady;

//                    NetworkSocket.Close();
//                    NetworkSocket.Dispose();

//                }
//                else
//                    State = Network.States.Ready;
//                return true;
//            }
//            else
//                return false;
//        }

//        public bool InPool = false;
//        public void BindAndReceive()
//        {
//            NetworkSocket.Bind(_endPoint);
//            Started = true;
//            // ReceiveAsync(new IPEndPoint(IPAddress.Any, 0), NetworkSocket);
//            Task.Run(delegate () { ReceiveAsync(new IPEndPoint(IPAddress.Any, 0), NetworkSocket, false); });

//        }
//        public void Start()
//        {
//            if (!InPool)
//                if (OnPacketReceived != null)
//                {
//                    switch (_connectionType)
//                    {
//                        case ConnectionType.UDP:
//                            {
//                                switch (_engine)
//                                {
//                                    case Engine.Native:
//                                        {

//                                            BindAndReceive();

//                                        }
//                                        break;
//                                    case Engine.PCAP:
//                                        {
//                                            Started = true;
//                                            Listening();
//                                        }
//                                        break;
//                                    default:
//                                        throw new Exception("Not Implemented yet");
//                                }

//                            }
//                            break;
//                        case ConnectionType.TCP:
//                            {
//                                switch (_engine)
//                                {
//                                    case Engine.Native:
//                                        {
//                                            NetworkSocket.Bind(_endPoint);
//                                            Started = true;
//                                            NetworkSocket.Listen();
//                                        }
//                                        break;
//                                    default:
//                                        throw new Exception("Not Implemented yet");
//                                }
//                            }
//                            break;
//                        case ConnectionType.SCTP:
//                            {
//                                switch (_engine)
//                                {
//                                    case Engine.Native:
//                                        {
//                                            NetworkSocket.Bind(_endPoint);
//                                            Started = true;
//                                        }
//                                        break;
//                                    default:
//                                        throw new Exception("Not Implemented yet");
//                                }
//                            }
//                            break;
//                    }

//                }
//                else
//                {
//                    throw new Exception("Please set OnPacketReceived before start.");
//                }

//        }
//    }
//}