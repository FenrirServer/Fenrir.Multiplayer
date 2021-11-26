using System.Net;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// General peer - identifies the remote connection
    /// </summary>
    public interface IPeer
    {
        /// <summary>
        /// Unique peer id
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Remote address used by this conection
        /// </summary>
        EndPoint EndPoint { get; }

        /// <summary>
        /// If set to true, outgoing messages will contain debug info.
        /// Setting this to true affects performance and should be disabled in production builds.
        /// </summary>
        bool WriteDebugInfo { get; set; }
    }
}
