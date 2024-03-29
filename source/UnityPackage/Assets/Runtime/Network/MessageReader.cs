﻿using System;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer
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
            // 1. [1 byte message type + flags]
            // 2. [8 bytes long message type hash]
            // 3. [1 byte channel number]
            // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
            // 5. [N bytes serialized message]

            messageWrapper = default;

            // 1. byte Message type + flags
            if (!byteStreamReader.TryReadByte(out byte typeAndFlagsCombined))
            {
                _logger.Warning("Malformed message: no message type and flags");
                return false;
            }
            byte messageTypeByte = (byte)(typeAndFlagsCombined >> 5);
            MessageType messageType = (MessageType)messageTypeByte;

            byte messageFlagsByte = (byte)(typeAndFlagsCombined & 0b111); // Clear front 5 bits to make sure conversion to MessageFlags works correctly
            MessageFlags messageFlags = (MessageFlags)messageFlagsByte;


            // 2. ulong Message type hash
            if (!byteStreamReader.TryReadULong(out ulong messageTypeHash))
            {
                _logger.Warning("Malformed message: no message type hash [long]");
                return false;
            }

            // 3. byte Channel Id
            if (!byteStreamReader.TryReadByte(out byte channel))
            {
                _logger.Warning("Malformed message: no channel id section");
                return false;
            }

            // 4. short request id
            short requestId = 0;
            if (messageType == MessageType.RequestWithResponse || messageType == MessageType.Response)
            {
                if (!byteStreamReader.TryReadShort(out requestId))
                {
                    _logger.Warning("Malformed message: no requestId section");
                    return false;
                }
            }

            // 5. string message Debug info
            string debugInfo = null;
            if(messageFlags.HasFlag(MessageFlags.IsDebug))
            {
                debugInfo = byteStreamReader.ReadString();
            }

            // Find message data type
            if (!_typeHashMap.TryGetTypeByHash(messageTypeHash, out Type dataType))
            {
                if(debugInfo != null)
                {
                    _logger.Warning("Unexpected message with type hash {0}. Message debug info: {1}", messageTypeHash, debugInfo);
                }
                else
                {
                    _logger.Warning("Unexpected message with type hash {0}", messageTypeHash);
                }
                return false;
            }

            // 6. byte[] Serialized message data
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

            // Validate incoming message type
            if(messageType == MessageType.Request && !(messageData is IRequest))
            {
                _logger.Warning($"Malformed message: message sent as Request but data type {dataType.Name} is not {nameof(IRequest)}");
                return false;
            }
            else if(messageType == MessageType.RequestWithResponse && !(messageData is IRequest))
            {
                _logger.Warning($"Malformed message: message sent as Request but message data type {dataType.Name} is not {nameof(IRequest)}");
                return false;
            }
            else if(messageType == MessageType.Response && !(messageData is IResponse))
            {
                _logger.Warning($"Malformed message: message sent as Response but message data type {dataType.Name} is not {nameof(IResponse)}");
                return false;
            }
            else if(messageType == MessageType.Event && !(messageData is IEvent))
            {
                _logger.Warning($"Malformed message: message sent as Event but message data type {dataType.Name} is not {nameof(IEvent)}");
                return false;
            }

            // Create message wrapper
            messageWrapper = new MessageWrapper(messageType, messageData, requestId, channel, messageFlags, debugInfo);

            return true;
        }
    }
}
