using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Event arguments invoked when new client connects to a server
    /// </summary>
    public class ServerPeerConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Server Peer
        /// </summary>
        public IServerPeer Peer { get; private set; }

        /// <summary>
        /// Creates new Server Peer Connected Event Args
        /// </summary>
        /// <param name="peer"></param>
        public ServerPeerConnectedEventArgs(IServerPeer peer)
        {
            Peer = peer;
        }
    }
}
