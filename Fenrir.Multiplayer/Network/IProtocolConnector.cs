
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
        /// True if connector is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// State of the connection
        /// </summary>
        ConnectionState State { get; }

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
        /// Serializer used for message serialization / deserialization
        /// </summary>
        IFenrirSerializer Serializer { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        IFenrirLogger Logger { get; set; }

        /// <summary>
        /// Time after which client is disconnected if no keep alive packets are received
        /// </summary>
        int DisconnectTimeout { get; set; }

        /// <summary>
        /// Delay between network ticks
        /// </summary>
        int UpdateTime { get; set; }

        /// <summary>
        /// Interval between KeepAlive packets.
        /// Must be smaller than <seealso cref="DisconnectTimeout"/>
        /// </summary>
        int PingInterval { get; set; }

        /// <summary>
        /// If set to true, packet loss is simulated and random packets will be dropped
        /// </summary>
        bool SimulatePacketLoss { get; set; }

        /// <summary>
        /// If set to true, delay is added for each packet
        /// </summary>
        bool SimulateLatency { get; set; }

        /// <summary>
        /// Chance of packet loss when packet loss simulation is enabled.
        /// </summary>
        int SimulationPacketLossChance { get; set; }

        /// <summary>
        /// Minimum simulated latency
        /// </summary>
        int SimulationMinLatency { get; set; }

        /// <summary>
        /// Maximum simulated latency
        /// </summary>
        int SimulationMaxLatency { get; set; }

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
    }
}
