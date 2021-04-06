namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Remote connection on the server (usually client)
    /// </summary>
    public interface IServerPeer : IPeer
    {
        /// <summary>
        /// Round-trip time of the packet
        /// </summary>
        int Latency { get; }

        /// <summary>
        /// Custom peer data that can be assigned to each peer
        /// </summary>
        object PeerData { get; set; }

        /// <summary>
        /// Notifies client of the event.
        /// All events are encrypted by default.
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="evt">Event object</param>
        /// <param name="channel">Channel number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        /// <param name="encrypted">True if message should be encrypted, otherwise false</param>
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
