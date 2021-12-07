using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Server.Events;
using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Remote connection on the server (usually client)
    /// </summary>
    public interface IServerPeer : IPeer
    {
        /// <summary>
        /// Invoked when peer is disconnected from the server
        /// </summary>
        event EventHandler<ServerPeerDisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Round-trip time of the packet
        /// </summary>
        int Latency { get; }

        /// <summary>
        /// Custom peer data that can be assigned to each peer
        /// </summary>
        object PeerData { get; set; }

        /// <summary>
        /// Custom connection request data. 
        /// Set only if custom Connection Request Handler is set on the <seealso cref="NetworkServer"/>.
        /// </summary>
        object ConnectionRequestData { get; }

        /// <summary>
        /// Notifies client of the event.
        /// All events are encrypted by default.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="evt">Event object</param>
        /// <param name="channel">Channel number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent;

        /// <summary>
        /// Notifies client of the event with specified encryption.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="encrypted">True if message should be encrypted, otherwise false</param>
        /// <param name="evt">Event object</param>
        /// <param name="channel">Channel number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendEvent<TEvent>(TEvent evt, bool encrypted, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent;

        /// <summary>
        /// Sends response to a request
        /// </summary>
        /// <typeparam name="TResponse">Type of response</typeparam>
        /// <param name="response">Response object</param>
        /// <param name="requestId">Request id</param>
        /// <param name="encrypted">True if message should be encrypted, otherwise false</param>
        /// <param name="channel">Channel number</param>
        /// <param name="ordered">If true, messages in the specified channel will arrive in order.</param>
        void SendResponse<TResponse>(TResponse response, short requestId, bool encrypted = true, byte channel = 0, bool ordered = true)
            where TResponse : IResponse;

        /// <summary>
        /// Disconnects the peer
        /// </summary>
        void Disconnect();
    }
}
