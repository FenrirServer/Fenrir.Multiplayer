using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
using System;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// LiteNet Message reader
    /// Reads incoming messages and deserializes them into a message wrapper
    /// </summary>
    class MessageReader
    {
        /// <summary>
        /// Serializer for serializing and deserializing messages
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
        /// Object pool of byte stream readers - used for incoming messages
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializer">Serializer</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        /// <param name="byteStreamReaderPool">Object pool of Byte Stream Readers</param>
        public MessageReader(INetworkSerializer serializer, ITypeHashMap typeHashMap, ILogger logger, RecyclableObjectPool<ByteStreamReader> byteStreamReaderPool)
        {
            _serializer = serializer;
            _typeHashMap = typeHashMap;
            _logger = logger;
            _byteStreamReaderPool = byteStreamReaderPool;
        }

        /// <summary>
        /// Reads an incoming message from byte stream (NetDataReader) and creates a message wrapper
        /// </summary>
        /// <param name="byteStreamReader">Byte stream reader with message data</param>
        /// <param name="messageWrapper">Message Wrapper that will be written if message can be read</param>
        /// <returns>True if message could be read, false otherwise</returns>
        public bool TryReadMessage(ByteStreamReader byteStreamReader, out MessageWrapper messageWrapper)
        {
            // TODO: Encryption

            // Message format: 
            // 1. [8 bytes long message type hash]
            // 2. [1 byte flags]
            // 3. [1 byte channel number]
            // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
            // 5. [N bytes serialized message]

            messageWrapper = default;

            // 1. ulong Message type hash
            if (!byteStreamReader.TryReadULong(out ulong messageTypeHash))
            {
                _logger.Warning("Malformed message: no message type hash [long]");
                return false;
            }

            // Find message type
            if(!_typeHashMap.TryGetTypeByHash(messageTypeHash, out Type dataType))
            {
                _logger.Warning("Malformed message: no message type with hash {0} in type hash map", messageTypeHash);
                return false;
            }

            // 2. byte Flags
            if(!byteStreamReader.TryReadByte(out byte flagBytes))
            {
                _logger.Warning("Malformed message: no flags section");
                return false;
            }

            MessageFlags messageFlags = (MessageFlags)flagBytes; // Flags enum

            // 3. byte Channel Id
            if (!byteStreamReader.TryReadByte(out byte channel))
            {
                _logger.Warning("Malformed message: no channel id section");
                return false;
            }

            // 4. short request id
            short requestId = 0;
            if (messageFlags.HasFlag(MessageFlags.HasRequestId))
            {
                if (!byteStreamReader.TryReadShort(out requestId))
                {
                    _logger.Warning("Malformed message: no requestId section");
                    return false;
                }
            }

            // 5. byte[] Serialized message data
            object messageData;
            try
            {
                messageData = _serializer.Deserialize(dataType, byteStreamReader); 
            }
            catch(SerializationException e)
            {
                _logger.Warning("Malformed message: failed to deserialize message {0}: {1}", dataType.Name, e);
                return false;
            }
            finally
            {
                byteStreamReader.SetNetDataReader(null);
                _byteStreamReaderPool.Return(byteStreamReader);
            }

            // Check data type
            MessageType messageType;
            if(messageData is IEvent)
            {
                messageType = MessageType.Event;
            }
            else if(messageData is IRequest)
            {
                messageType = MessageType.Request;
            }
            else if(messageData is IResponse)
            {
                messageType = MessageType.Response;
            }
            else
            {
                _logger.Warning("Malformed message: unknown message type {0}, must be Event, Request or Response", dataType.Name);
                return false;
            }

            // Create message wrapper
            messageWrapper = new MessageWrapper(messageType, messageData, requestId, channel, messageFlags);

            return true;
        }
    }
}
