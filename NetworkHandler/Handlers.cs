using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkHandler
{
    public class NetworkEventHandler : EventArgs
    {
        public byte[] Packet;
        public int TransferedBytes;
        public IPEndPoint EndPoint;
        public NetworkBase Network;
    }
    public class NetworkErrorEventHandler : EventArgs
    {
        public string Error;
        public Exception exception;
    }
}
