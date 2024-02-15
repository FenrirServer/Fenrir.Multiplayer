using System.Collections.Generic;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Represents a protocol server listener
    /// </summary>
    public interface IProtocolListener
    {
        /// <summary>
        /// Indicates if protocol is listening
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Type of the protocol
        /// </summary>
        ProtocolType ProtocolType { get; }

        /// <summary>
        /// Returns protocol connection data, required to pass by the client when connecting using this protocol
        /// </summary>
        IProtocolConnectionData GetConnectionData();

        /// <summary>
        /// Connected peers
        /// </summary>
        IEnumerable<IServerPeer> Peers { get; }

        /// <summary>
        /// Number of connected peers
        /// </summary>
        int NumPeers { get; }
    }
}