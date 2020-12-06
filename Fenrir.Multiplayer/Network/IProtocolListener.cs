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
        /// Connection data, required to pass by the client when connecting using this protocol
        /// </summary>
        IProtocolConnectionData ConnectionData { get; }

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
    }
}