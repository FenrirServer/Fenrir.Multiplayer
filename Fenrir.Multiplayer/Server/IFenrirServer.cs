using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
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
        /// Starts the server
        /// </summary>
        /// <returns>Task that completes when server has started</returns>
        Task Start();

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <returns>Task that completes when server has stopped</returns>
        Task Stop();

        /// <summary>
        /// Sets contract serializer. 
        /// If not set, IByteStreamSerializable is the only supported way of serialization.
        /// If set, any data contract will be serialized using that contract serializer,
        /// with IByteStreamSerializable used as a fall back.
        /// </summary>
        void SetContractSerializer(IContractSerializer contractSerializer);

        /// <summary>
        /// Sets Fenrir Logger. If not set, EventBasedLogger is used
        /// </summary>
        /// <param name="logger">Fenrir Logger</param>
        void SetLogger(IFenrirLogger logger);
    }
}