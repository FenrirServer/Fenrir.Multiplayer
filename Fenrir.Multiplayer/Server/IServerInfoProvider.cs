using Fenrir.Multiplayer.Network;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Provides information about Network Server
    /// </summary>
    public interface IServerInfoProvider
    {
        /// <summary>
        /// Public server hostname. 
        /// Clients will use this hostname to connect.
        /// By default, bind on 0.0.0.0
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
        /// True if server is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Listeners available on this server
        /// </summary>
        IEnumerable<IProtocolListener> Listeners { get; }
    }
}