using Fenrir.Multiplayer.Events;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Fenrir Network Client
    /// Connects to a NetworkServer using given protocols
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// Invoked when client is disconnected
        /// </summary>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Invoked when network error occurs
        /// </summary>
        event EventHandler<NetworkErrorEventArgs> NetworkError;

        /// <summary>
        /// Unique id of the client
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Protocols enabled on this client
        /// </summary>
        ProtocolType EnabledProtocols { get; set; }

        /// <summary>
        /// Client Peer object. null if client is not connected
        /// </summary>
        IClientPeer Peer { get; }
        
        /// <summary>
        /// State of the connection
        /// </summary>
        ConnectionState State { get; }

        /// <summary>
        /// Network Serializer
        /// </summary>
        INetworkSerializer Serializer { get; }

        /// <summary>
        /// Connects using Server Info URI
        /// Server Info URI will be queried to obtain server data
        /// </summary>
        /// <param name="serverInfoUri">Server Info URI to query</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ConnectionResponse> Connect(Uri serverInfoUri, object connectionRequestData = null);

        /// <summary>
        /// Connects using Server Info URI
        /// Server Info URI will be queried to obtain server data
        /// </summary>
        /// <param name="serverInfoUri">Server Info URI to query</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ConnectionResponse> Connect(string serverInfoUri, object connectionRequestData = null);

        /// <summary>
        /// Connects using Server Info object
        /// Directly connects to a server with existing Server Info object
        /// </summary>
        /// <param name="serverInfo">Server Info object - contains information about server</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ConnectionResponse> Connect(ServerInfo serverInfo, object connectionRequestData = null);

        /// <summary>
        /// Disconnects from the Server
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
        /// Adds a factory method for a serializable type. If factory is not set, new instances are created using <seealso cref="Activator.CreateInstance(Type)"/>
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="factoryMethod">Factory method</param>
        void AddSerializableTypeFactory<T>(Func<T> factoryMethod) where T : IByteStreamSerializable;
    }
}
