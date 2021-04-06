using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Type of the protocol
    /// </summary>
    [Flags]
    public enum ProtocolType : byte
    {
        /// <summary>
        /// No protocol supported
        /// </summary>
        None = 0,

        /// <summary>
        /// LiteNet protocol is reliable UDP 
        /// protocol that supports all deliery methods
        /// </summary>
        LiteNet = 1,

        /// <summary>
        /// WebSocket Protocol
        /// Used as a fallback when UDP is not supported
        /// </summary>
        WebSocket = 2,

        /// <summary>
        /// All Protocols
        /// </summary>
        All = LiteNet | WebSocket
    }
}
