using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Server.Events
{
    /// <summary>
    /// Event arguments invoked when new client connects to a server
    /// </summary>
    public class ServerPeerConnectedEventArgs : EventArgs
    {
        public IServerPeer Peer { get; private set; }

        public ServerPeerConnectedEventArgs(IServerPeer peer)
        {
            Peer = peer;
        }
    }
}
