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
        /// Notifies client of the event
        /// </summary>
        /// <typeparam name="TEvent">Type of the event</typeparam>
        /// <param name="evt">Event object</param>
        /// <param name="channel">Channel number</param>
        /// <param name="deliveryMethod">Delivery method</param>
        void SendEvent<TEvent>(TEvent evt, byte channel = 0, MessageDeliveryMethod deliveryMethod = MessageDeliveryMethod.ReliableOrdered)
            where TEvent : IEvent;
    }
}
