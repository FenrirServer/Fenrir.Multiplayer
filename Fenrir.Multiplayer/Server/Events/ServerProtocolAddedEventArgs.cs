using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Server.Events
{
    /// <summary>
    /// Invoked when protocol listener is added to <seealso cref="FenrirServer"/>
    /// </summary>
    public class ServerProtocolAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Protocol listener added
        /// </summary>
        public IProtocolListener ProtocolListener { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="protocolListener">Protocol Listener</param>
        public ServerProtocolAddedEventArgs(IProtocolListener protocolListener)
        {
            ProtocolListener = protocolListener;
        }
    }
}
