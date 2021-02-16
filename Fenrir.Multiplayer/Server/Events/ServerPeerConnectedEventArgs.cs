using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Server.Events
{
    /// <summary>
    /// Event arguments invoked when new client disconnectes from a server
    /// </summary>
    public class ServerPeerDisconnectedEventArgs : EventArgs
    {
        public IServerPeer Peer { get; private set; }

        public ServerPeerDisconnectedEventArgs(IServerPeer peer)
        {
            Peer = peer;
        }
    }
}
