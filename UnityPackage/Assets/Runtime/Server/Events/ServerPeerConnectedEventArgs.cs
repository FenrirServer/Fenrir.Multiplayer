using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Server.Events
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
