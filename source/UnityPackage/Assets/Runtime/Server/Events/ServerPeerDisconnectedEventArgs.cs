using System;

namespace Fenrir.Multiplayer
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
