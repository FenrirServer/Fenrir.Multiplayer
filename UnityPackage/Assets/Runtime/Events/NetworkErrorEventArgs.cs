using System;
using System.Net;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Events
{
    /// <summary>
    /// Event arguments object that contains detailed information about client network error 
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
