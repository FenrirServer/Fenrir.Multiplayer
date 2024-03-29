﻿using System.Net.Sockets;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Thrown when client fails to connect to a server
    /// </summary>
    public class ConnectionFailedException : FenrirException
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
        /// <param name="message">Disconnect message</param>
        /// <param name="reason">Disconnect reason</param>
        /// <param name="socketError">Socket error</param>
        public ConnectionFailedException(string message, DisconnectedReason reason, SocketError socketError)
            : base(message)
        {
            Reason = reason;
            SocketError = socketError;
        }
    }
}
