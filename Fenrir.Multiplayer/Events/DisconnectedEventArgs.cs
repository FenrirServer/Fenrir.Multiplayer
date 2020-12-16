using Fenrir.Multiplayer.Network;
using System;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Events
{
    /// <summary>
    /// Event args invoked when client is disconnected
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Disconnect reason
        /// </summary>
        public DisconnectedReason Reason { get; set; }
        
        /// <summary>
        /// Additional information about socket error
        /// </summary>
        public SocketError SocketError { get; set; }

        /// <summary>
        /// If disconnected by the server, contains
        /// additional data sent by the server (usually disconnect message)
        /// </summary>
        public object DisconnectData { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="reason">Disconnect reason</param>
        /// <param name="socketError">Socket error</param>
        /// <param name="data">Disconnect data</param>
        public DisconnectedEventArgs(DisconnectedReason reason, SocketError socketError, object data)
        {
            Reason = reason;
            SocketError = socketError;
            DisconnectData = data;
        }
    }
}
