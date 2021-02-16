using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Network
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
        /// <param name="byteStreamWriterPool">Object pool of Byte Stream Writers</param>
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
            // 1. [8 bytes long message type hash]
            // 2. [1 byte flags]
            // 3. [1 byte channel number]
            // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
            // 5. [N bytes serialized message]


            // 1. ulong Message type hash
            ulong messageTypeHash = _typeHashMap.GetTypeHash(messageWrapper.MessageData.GetType());
            byteStreamWriter.Write(messageTypeHash); // Type hash

            // 2. byte Message flags
            byteStreamWriter.Write((byte)messageWrapper.Flags);

            // 3. byte Channel number
            byteStreamWriter.Write(messageWrapper.Channel);

            // 4. short Request id
            if (messageWrapper.Flags.HasFlag(MessageFlags.HasRequestId))
            {
                byteStreamWriter.Write(messageWrapper.RequestId);
            }

            // 5. byte[] Serialized message
            _serializer.Serialize(messageWrapper.MessageData, byteStreamWriter); // Serialize into remaining bytes
        }
    }
}
