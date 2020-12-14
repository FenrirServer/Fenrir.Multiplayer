namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Simple structure that stores message and it's meta information
    /// </summary>
    internal struct MessageWrapper
    {
        /// <summary>
        /// Type of the message
        /// </summary>
        public MessageType MessageType;

        /// <summary>
        /// Id of the requet, if message type is <see cref="IRequest"/>
        /// </summary>
        public int RequestId;

        /// <summary>
        /// Message data object
        /// </summary>
        public object MessageData;

        /// <summary>
        /// Peer associated with that message
        /// </summary>
        public IPeerInternal Peer;

        /// <summary>
        /// Channel of the message
        /// </summary>
        public byte Channel;
        
        /// <summary>
        /// Delivery method of the message
        /// </summary>
        public MessageDeliveryMethod DeliveryMethod;
    }
}
