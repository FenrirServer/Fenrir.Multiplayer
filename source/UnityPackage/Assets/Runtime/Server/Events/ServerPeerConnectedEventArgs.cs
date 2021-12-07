using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Event arguments invoked when new client disconnectes from a server
    /// </summary>
    public class ServerPeerDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Peer that was disconnected
        /// </summary>
        public IServerPeer Peer { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="peer">Peer that was disconnected</param>
        public ServerPeerDisconnectedEventArgs(IServerPeer peer)
        {
            Peer = peer;
        }
    }
}
