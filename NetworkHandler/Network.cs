
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace NetworkHandler
{

    public abstract class NetworkBase
    {
        /// <summary>
        /// Removing all receive packet handler
        /// </summary>
        /// 
        public void RemoveHandlers()
        {
            foreach (var x in this.OnPacketReceived.GetInvocationList())
            {
                this.OnPacketReceived -= (OnPacketReceivedHandler)x;
            }
        }
        /// <summary>
        /// Check if socket already started
        /// </summary>
        internal bool Started = false;
        /// <summary>
        /// Receive handler
        /// </summary>
        public delegate void OnPacketReceivedHandler(object sender, NetworkEventHandler e);
        /// <summary>
        /// Register to receive packets
        /// </summary>
        public event OnPacketReceivedHandler OnPacketReceived;
        /// <summary>
        /// To invoke receiving in abstract class
        /// </summary>
        protected virtual void OnPacketReceivedCall(object sender, NetworkEventHandler e)
        {
            OnPacketReceived?.Invoke(sender, e);
        }
        /// <summary>
        /// Error Event Handler
        /// </summary>
        public delegate void OnErrorHandler(object sender, NetworkErrorEventHandler e);
        /// <summary>
        /// Receive Error
        /// </summary>
        public event OnErrorHandler OnError;
        /// <summary>
        /// To invoke error event in abstract class
        /// </summary>
        protected virtual void OnErrorCall(object sender, NetworkErrorEventHandler e)
        {
            OnError?.Invoke(sender, e);
        }
        /// <summary>
        /// Network Mode (Client, Server)
        /// </summary>
        public enum Modes
        {
            Client,Server
        }
      

        /// <summary>
        /// Sent packet to specified endpoint
        /// </summary>
        /// <param name="endPoint">Destination Endpoint who will receive the Data</param>
        /// <param name="Data">Array of byte to send to peer</param>
        /// <param name="Length">Length of data</param>
        /// <param name="WaitForResponse">Select whether client should wait for peer response or not in client mode</param>
        public abstract void Send(IPEndPoint endPoint, byte[] Data, int Length, bool WaitForResponse);
        /// <summary>
        /// Stop Network
        /// </summary>
        public abstract bool Stop();
        /// <summary>
        /// Start Network
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// <para>Listening IP and port in Server Mode</para>
        /// <para>Source IP and port in Client Mode</para>
        /// </summary>

        internal IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);
        /// <summary>
        /// <para>Client Mode: Connecting to specified destination</para>
        /// <para>Server Mode: Allow multiple source to connect to specified Listening IP and port</para>
        /// </summary>

        internal Modes _mode = Modes.Server;

    }
}