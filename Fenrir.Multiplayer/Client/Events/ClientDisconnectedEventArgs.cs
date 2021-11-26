using Fenrir.Multiplayer.Network;
using System;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Client.Events
{
    /// <summary>
    /// Event arguments object that contains detailed information about client disconect event
    /// </summary>
    public class ClientDisconnectedEventArgs : EventArgs
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
        /// Default constructor
        /// </summary>
        /// <param name="reason">Disconnect reason</param>
        /// <param name="socketError">Socket error</param>
        public ClientDisconnectedEventArgs(DisconnectedReason reason, SocketError socketError)
        {
            Reason = reason;
            SocketError = socketError;
        }
    }
}
