using System;
using System.Net;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Events
{
    /// <summary>
    /// Event args invoked when network error occurs
    /// </summary>
    public class NetworkErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Remote endpoint
        /// </summary>
        public IPEndPoint Endpoint { get; set; }

        /// <summary>
        /// Socket error
        /// </summary>
        public SocketError SocketError { get; set; }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="endpoint">Remote endpoint</param>
        /// <param name="socketError">Socket error</param>
        public NetworkErrorEventArgs(IPEndPoint endpoint, SocketError socketError)
        {
            Endpoint = endpoint;
            SocketError = socketError;
        }
    }
}
