using Fenrir.Multiplayer.Network;
using System;
using System.Net.Sockets;

namespace Fenrir.Multiplayer.Events
{
    public class DisconnectedEventArgs : EventArgs
    {
        public DisconnectedReason Reason { get; set; }
        
        public SocketError SocketError { get; set; }

        public object DisconnectData { get; set; }

        public DisconnectedEventArgs(DisconnectedReason reason, SocketError socketError, object data)
        {
            Reason = reason;
            SocketError = socketError;
            DisconnectData = data;
        }
    }
}
