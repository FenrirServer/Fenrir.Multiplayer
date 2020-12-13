using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Represents a protocol server listener
    /// </summary>
    public interface IProtocolListener : IDisposable
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
        /// Starts protocol listener
        /// </summary>
        /// <returns>Task that completes when protocol listener is runnign</returns>
        Task Start();

        /// <summary>
        /// Stops the protocol listener
        /// </summary>
        /// <returns>Task that completes when protocol listener is stopped</returns>
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


        /// <summary>
        /// Returns protocol connection data, required to pass by the client when connecting using this protocol
        /// </summary>
        IProtocolConnectionData GetConnectionData();
    }
}