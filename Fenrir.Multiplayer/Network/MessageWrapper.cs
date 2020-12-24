namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Simple structure that stores message and it's metadata
    /// </summary>
    internal struct MessageWrapper
    {
        /// <summary>
        /// Type of the message: Event, Request or Response
        /// </summary>
        public MessageType MessageType;

        /// <summary>
        /// Message data object. 
        /// If <see cref="MessageType"/> is <see cref="MessageType.Request"/>, should be <see cref="IRequest"/>.
        /// If <see cref="MessageType"/> is <see cref="MessageType.Response"/>, should be <see cref="IResponse"/>.
        /// If <see cref="MessageType"/> is <see cref="MessageType.Event"/>, should be <see cref="IEvent"/>.
        /// </summary>
        public object MessageData;

        /// <summary>
        /// Id of the request, if <see cref="MessageType"/> is <see cref="MessageType.Request"/> or <see cref="MessageType.Response"/>,
        /// and <see cref="MessageData"/> type is <see cref="IRequest"/> or <see cref="IResponse"/>
        /// </summary>
        public short RequestId;

        /// <summary>
        /// Channel of the message
        /// </summary>
        public byte Channel;

        /// <summary>
        /// Flags of the message
        /// </summary>
        public MessageFlags Flags;

        /// <summary>
        /// Delivery method of the message. Only set for outgoing messages.
        /// </summary>
        public MessageDeliveryMethod DeliveryMethod;


        #region Constructor
        /// <summary>
        /// Creates new outgoing Message Wrapper
        /// </summary>
        /// <param name="messageType">Message Type. <seealso cref="MessageType"/></param>
        /// <param name="messageData">Message Data object. <seealso cref="MessageData"/></param>
        /// <param name="requestId">Id of the request. <seealso cref="RequestId"/></param>
        /// <param name="channel">Channel number. <see cref="Channel"/></param>
        /// <param name="flags">Message flags. <see cref="Flags"/></param>
        /// <param name="deliveryMethod">Delivery method. <seealso cref="DeliveryMethod"/></param>
        public MessageWrapper(MessageType messageType, object messageData, short requestId, byte channel, MessageFlags flags, MessageDeliveryMethod deliveryMethod)
        {
            MessageType = messageType;
            MessageData = messageData;
            RequestId = requestId;
            Channel = channel;
            Flags = flags;
            DeliveryMethod = deliveryMethod;
        }

        /// <summary>
        /// Creates new incoming Message Wrapper
        /// </summary>
        /// <param name="messageType">Message Type. <seealso cref="MessageType"/></param>
        /// <param name="messageData">Message Data object. <seealso cref="MessageData"/></param>
        /// <param name="requestId">Id of the request. <seealso cref="RequestId"/></param>
        /// <param name="channel">Channel number. <see cref="Channel"/></param>
        /// <param name="flags">Message flags. <see cref="Flags"/></param>
        /// <param name="deliveryMethod">Delivery method. <seealso cref="DeliveryMethod"/></param>
        public MessageWrapper(MessageType messageType, object messageData, short requestId, byte channel, MessageFlags flags)
            : this(messageType, messageData, requestId, channel, flags, default)
        {
        }
        #endregion

        #region Factory Methods
        /// <summary>
        /// Credates message wrapper for an event
        /// </summary>
        /// <param name="data">Eveny data. <seealso cref="MessageData"/></param>
        /// <param name="channel">Channel number. <seealso cref="Channel"/></param>
        /// <param name="flags">Message flags. <see cref="Flags"/></param>
        /// <param name="deliveryMethod">Delivery method. <seealso cref="DeliveryMethod"/></param>
        /// <returns>New MessageWrapper that wraps given event</returns>
        public static MessageWrapper WrapEvent(IEvent data, byte channel, MessageFlags flags, MessageDeliveryMethod deliveryMethod)
        {
            return new MessageWrapper(MessageType.Event, data, 0, channel, flags, deliveryMethod);
        }

        /// <summary>
        /// Credates message wrapper for a request
        /// </summary>
        /// <param name="data">Request data. <seealso cref="MessageData"/></param>
        /// <param name="requestId">Request id. <seealso cref="RequestId"/></param>
        /// <param name="channel">Channel number. <seealso cref="Channel"/></param>
        /// <param name="flags">Message flags. <see cref="Flags"/></param>
        /// <param name="deliveryMethod">Delivery method. <seealso cref="DeliveryMethod"/></param>
        /// <returns>New MessageWrapper that wraps given request</returns>
        public static MessageWrapper WrapRequest(IRequest data, short requestId, byte channel, MessageFlags flags, MessageDeliveryMethod deliveryMethod)
        {
            return new MessageWrapper(MessageType.Request, data, requestId, channel, flags, deliveryMethod);
        }

        /// <summary>
        /// Credates message wrapper for a response
        /// </summary>
        /// <param name="data">Response data. <seealso cref="MessageData"/></param>
        /// <param name="requestId">Request id. <seealso cref="RequestId"/></param>
        /// <param name="channel">Channel number. <seealso cref="Channel"/></param>
        /// <param name="flags">Message flags. <see cref="Flags"/></param>
        /// <param name="deliveryMethod">Delivery method. <seealso cref="DeliveryMethod"/></param>
        /// <returns>New MessageWrapper that wraps given response</returns>
        public static MessageWrapper WrapResponse(IResponse data, short requestId, byte channel, MessageFlags flags, MessageDeliveryMethod deliveryMethod)
        {
            return new MessageWrapper(MessageType.Response, data, requestId, channel, flags, deliveryMethod);
        }
        #endregion
    }
}
