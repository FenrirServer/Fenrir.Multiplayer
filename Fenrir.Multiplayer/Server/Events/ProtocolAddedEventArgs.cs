using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Server.Events
{
    /// <summary>
    /// Invoked when protocol listener is added to Fenrir Server
    /// </summary>
    public class ProtocolAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Protocol listener added
        /// </summary>
        public IProtocolListener ProtocolListener { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="protocolListener">Protocol Listener</param>
        public ProtocolAddedEventArgs(IProtocolListener protocolListener)
        {
            ProtocolListener = protocolListener;
        }
    }
}
