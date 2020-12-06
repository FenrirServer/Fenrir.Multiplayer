using Fenrir.Multiplayer.Network;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Fenrir Server Host
    /// </summary>
    public interface IFenrirServer
    {
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

        /// <summary>
        /// Adds protocol to the server
        /// </summary>
        /// <param name="protocol">Protocol</param>
        void AddProtocol(IProtocol protocol);

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <returns>Task that completes when server has started</returns>
        Task Start();

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <returns>Task that completes when server has stopped</returns>
        Task Stop();
    }
}