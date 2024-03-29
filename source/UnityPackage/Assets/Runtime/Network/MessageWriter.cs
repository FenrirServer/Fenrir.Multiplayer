﻿namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Network Message Writer
    /// Wrapper around byte stream writer and Message Wrapper. Writes messages into a byte stream.
    /// </summary>
    class MessageWriter
    {
        /// <summary>
        /// Serialization provider - used for serializing and deserializing messages
        /// </summary>
        private readonly INetworkSerializer _serializer;

        /// <summary>
        /// Type map - contians list of types and hashes
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        public MessageWriter(INetworkSerializer serializer, ITypeHashMap typeHashMap, ILogger logger)
        {
            _serializer = serializer;
            _typeHashMap = typeHashMap;
            _logger = logger;
        }

        /// <summary>
        /// Writes an outgoing message wrapper into a byte stream
        /// </summary>
        /// <param name="byteStreamWriter">ByteStreamWriter to write into</param>
        /// <param name="messageWrapper">Message Wrapper - outgoing message</param>
        public void WriteMessage(ByteStreamWriter byteStreamWriter, MessageWrapper messageWrapper)
        {
            // TODO: Encryption

            // Message format: 
            // 1. [1 byte message type + flags]
            // 2. [8 bytes long message type hash]
            // 3. [1 byte channel number]
            // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
            // 5. [N bytes serialized message]

            // 1. byte Message type + flags
            byte typeAndFlagsCombined = (byte)messageWrapper.MessageType;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)messageWrapper.Flags);
            byteStreamWriter.Write(typeAndFlagsCombined);

            // 2. ulong Message type hash
            ulong messageTypeHash = _typeHashMap.GetTypeHash(messageWrapper.MessageData.GetType());
            byteStreamWriter.Write(messageTypeHash); // Type hash

            // 3. byte Channel number
            byteStreamWriter.Write(messageWrapper.Channel);

            // 4. short Request id
            if (messageWrapper.MessageType == MessageType.RequestWithResponse || messageWrapper.MessageType == MessageType.Response)
            {
                byteStreamWriter.Write(messageWrapper.RequestId);
            }

            // 5. string message Debug info
            if(messageWrapper.Flags.HasFlag(MessageFlags.IsDebug))
            {
                byteStreamWriter.Write(messageWrapper.GetDebugInfo());
            }

            // 6. byte[] Serialized message
            _serializer.Serialize(messageWrapper.MessageData, byteStreamWriter); // Serialize into remaining bytes
        }
    }
}
