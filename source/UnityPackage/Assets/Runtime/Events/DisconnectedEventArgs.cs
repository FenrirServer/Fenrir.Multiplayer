﻿using System;
using System.Net.Sockets;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Event arguments object that contains detailed information about client disconect event
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
        /// Default constructor
        /// </summary>
        /// <param name="reason">Disconnect reason</param>
        /// <param name="socketError">Socket error</param>
        public DisconnectedEventArgs(DisconnectedReason reason, SocketError socketError)
        {
            Reason = reason;
            SocketError = socketError;
        }
    }
}
