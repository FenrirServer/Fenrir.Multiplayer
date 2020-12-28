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
    }
}
