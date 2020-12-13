
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Client
{
    /// <summary>
    /// Fenrir Client
    /// Connects to a FenrirServer using given protocols
    /// </summary>
    public interface IFenrirClient
    {
        /// <summary>
        /// Unique id of the client
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Client Peer object. null if client is not connected
        /// </summary>
        IClientPeer Peer { get; }
        
        /// <summary>
        /// State of the connection
        /// </summary>
        ConnectionState State { get; }

        /// <summary>
        /// Connects using Server Info URI
        /// Server Info URI will be queried to obtain server data
        /// </summary>
        /// <param name="serverInfoUri">Server Info URI to query</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ConnectionResponse> Connect(Uri serverInfoUri, object connectionRequestData = null);

        /// <summary>
        /// Connects using Server Info object
        /// Directly connects to a server with existing Server Info object
        /// </summary>
        /// <param name="serverInfo">Server Info object - contains information about server</param>
        /// <param name="connectionRequestData">Custom connection request data</param>
        /// <returns>Connection Result</returns>
        Task<ConnectionResponse> Connect(ServerInfo serverInfo, object connectionRequestData = null);

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
