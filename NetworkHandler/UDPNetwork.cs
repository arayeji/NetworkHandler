
using System.Net;
using System.Net.Sockets;

namespace NetworkHandler
{
    public class UDPNetwork : NetworkBase
    {
        
         
        Socket NetworkSocket ;
       


        public UDPNetwork(IPEndPoint endPoint , Modes mode, AddressFamily addressFamily)
        {
          
            _endPoint = endPoint;
            _mode = mode;
             
            NetworkSocket = new Socket(addressFamily, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);

        }

        public override void Send(IPEndPoint endPoint, byte[] Data, int Length, bool WaitForResponse)
        {

            if (Started)
            {
                SendAsync(endPoint, Data, Length, WaitForResponse);
            }
        }
        void SendAsync(IPEndPoint endPoint, byte[] Data, int Length, bool WaitForResponse)
        {
            if (Started)
            {
                SocketAsyncEventArgs rServerAccEvent = new SocketAsyncEventArgs();
                rServerAccEvent.RemoteEndPoint = endPoint;
                Socket So = NetworkSocket;


                rServerAccEvent.Completed += delegate (object? sender, SocketAsyncEventArgs e)
                {
                    if (WaitForResponse && Started)
                        ReceiveAsync(endPoint, So, true);

                };
                rServerAccEvent.SetBuffer(Data, 0, Length);

                bool ok = So.SendToAsync(rServerAccEvent);
                if (!ok)
                {

                    if (WaitForResponse && Started)
                        ReceiveAsync(endPoint, So, true);

                }
                rServerAccEvent.Dispose();
            }
        }
         

        public void RServerAccEvent_Completed(object? sender, SocketAsyncEventArgs e)
        {
            
            try
            {

                if (Started)
                {

                    if (e.BytesTransferred > 0)
                    {
                        OnPacketReceivedCall(sender, new NetworkEventHandler { Network = this, Packet = e.Buffer, TransferedBytes = e.BytesTransferred, EndPoint = (IPEndPoint)e.RemoteEndPoint });

                        e.Dispose();
                        if (!(bool)e.UserToken)
                            ReceiveAsync(new IPEndPoint(IPAddress.Any, 0), (Socket)(sender), (bool)e.UserToken);

                    }
                }
                else
                    e.Dispose();

            }
            catch
            {

            }

        }
        void ReceiveAsync(IPEndPoint endPoint, Socket So, bool OneTime)
        {

            while (Started)
            {

                if (So != null)
                {
                    try
                    {
                        SocketAsyncEventArgs rServerAccEvent = new SocketAsyncEventArgs();
                        byte[] buffer = new byte[1600]; // Buffer Size to receive. You can change based on MTU size for tuning performance

                        rServerAccEvent.UserToken = OneTime;
                        rServerAccEvent.RemoteEndPoint = endPoint;
                        rServerAccEvent.Completed += RServerAccEvent_Completed;
                        rServerAccEvent.SetBuffer(buffer, 0, 1600);

                        bool ok = So.ReceiveFromAsync(rServerAccEvent);
                        if (!ok)
                        {
                             
                            try
                            {

                                if (Started)
                                {

                                    if (rServerAccEvent.BytesTransferred > 0)
                                    {
                                        OnPacketReceivedCall(So, new NetworkEventHandler { Network = this, Packet = rServerAccEvent.Buffer, TransferedBytes = rServerAccEvent.BytesTransferred, EndPoint = (IPEndPoint)rServerAccEvent.RemoteEndPoint });
                                        rServerAccEvent.Dispose();

                                    }
                                }
                                else
                                    rServerAccEvent.Dispose();

                            }
                            catch
                            {

                            }


                        }
                        else
                            break;

                       

                    }
                    catch (System.ObjectDisposedException) //On Network Stop error
                    {

                    }
                }

            }
        }

        public override bool Stop()
        {
            if (Started)
            {
                Started = false;
                RemoveHandlers();
                NetworkSocket.Close();
                NetworkSocket.Dispose();
                return true;
            }
            else
                return false;
        }

      
        public void BindAndReceive()
        {
            NetworkSocket.Bind(_endPoint);
            Started = true;
            Task.Run(delegate () { ReceiveAsync(new IPEndPoint(IPAddress.Any, 0), NetworkSocket, false); });

        }
        public override void Start()
        {
            BindAndReceive();
        }
    }
}
