using System;
using System.Net;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Events
{
    public class NetworkErrorEventArgs : EventArgs
    {
        public IPEndPoint Endpoint { get; set; }

        public SocketError SocketError { get; set; }
        
        public NetworkErrorEventArgs(IPEndPoint endpoint, SocketError socketError)
        {
            Endpoint = endpoint;
            SocketError = socketError;
        }
    }
}
