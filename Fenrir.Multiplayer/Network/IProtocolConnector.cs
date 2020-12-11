
using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Events;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
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
        /// Protocol Type
        /// </summary>
        ProtocolType ProtocolType { get; }

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
        /// Type of the protocol-specific connection data
        /// </summary>
        Type ConnectionDataType { get; }

        /// <summary>
        /// Connects using protocol-specific implementation
        /// </summary>
        /// <param name="connectionRequest">Connection Request</param>
        /// <returns>Result of the connection</returns>
        Task<ConnectionResponse> Connect(ClientConnectionRequest connectionRequest);

        /// <summary>
        /// Disconnects
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Adds event handler to the client
        /// Event handler will be invoked when event of type TEvent is received from the server
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="eventHandler">Event handler</param>
        void AddEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent;

        /// <summary>
        /// Removed event handler from the client
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="eventHandler">Event handler</param>
        void RemoveEventHandler<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IEvent;


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
