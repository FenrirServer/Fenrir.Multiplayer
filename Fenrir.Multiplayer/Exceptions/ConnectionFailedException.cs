using Fenrir.Multiplayer.Network;
using System;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Exceptions
{
    public class ConnectionFailedException : FenrirException
    {
        public DisconnectedReason Reason { get; set; }

        public SocketError SocketError { get; set; }

        public object DisconnectData { get; set; }

        public ConnectionFailedException(string message, DisconnectedReason reason, SocketError socketError, object data = null)
            : base(message)
        {
            Reason = reason;
            SocketError = socketError;
            DisconnectData = data;
        }
    }
}
