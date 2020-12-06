
using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Events;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Protocol connector
    /// Connects to a IProtocolListener using specific protocol implementation
    /// </summary>
    public interface IProtocolConnector : IDisposable
    {
        /// <summary>
        /// Invoked when protocol is disconnected
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Invoked when network error occurs
        /// </summary>
        event EventHandler<NetworkErrorEventArgs> NetworkError;

        /// <summary>
        /// State of the connection
        /// </summary>
        ConnectorState State { get; }

        /// <summary>
        /// Latency between client and server (packet round-trip time)
        /// </summary>
        int Latency { get; }

        /// <summary>
        /// Client Peer object, represents an active connection
        /// null if client is not connected
        /// </summary>
        IClientPeer Peer { get; }

        /// <summary>
        /// Connects using protocol-specific implementation
        /// </summary>
        /// <param name="connectionRequest">Connection Request</param>
        /// <returns>Result of the connection</returns>
        Task<ClientConnectionResult> Connect(ClientConnectionRequest connectionRequest);

        /// <summary>
        /// Disconnects
        /// </summary>
        void Disconnect();
    }
}
