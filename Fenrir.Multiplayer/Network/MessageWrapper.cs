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
        /// True if message is encrypted
        /// </summary>
        public bool IsEncrypted;

        /// <summary>
        /// Id of the requet, if message type is <see cref="IRequest"/>
        /// </summary>
        public ushort RequestId;

        /// <summary>
        /// Message data object
        /// </summary>
        public object MessageData;

        /// <summary>
        /// Peer associated with that message
        /// </summary>
        public IPeerInternal Peer;

        /// <summary>
        /// Channel of the message. Only set for outgoing messages
        /// </summary>
        public byte Channel;
        
        /// <summary>
        /// Delivery method of the message. Only set for outgoing messages
        /// </summary>
        public MessageDeliveryMethod DeliveryMethod;
    }
}
