using Fenrir.Multiplayer.Network;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Provides information about Fenrir Server
    /// </summary>
    public interface IFenrirServerInfoProvider
    {
        /// <summary>
        /// Public server hostname. 
        /// Clients will use to connect
        /// </summary>
        string Hostname { get; set; }

        /// <summary>
        /// Unique Id of the server
        /// </summary>
        string ServerId { get; set; }

        /// <summary>
        /// Status of the server
        /// </summary>
        ServerStatus Status { get; }

        /// <summary>
        /// Listeners available on this server
        /// </summary>
        IEnumerable<IProtocolListener> Listeners { get; }
    }
}