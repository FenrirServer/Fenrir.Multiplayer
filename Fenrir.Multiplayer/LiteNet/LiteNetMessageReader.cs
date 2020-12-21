using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using System;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.LiteNet
{
    /// <summary>
    /// LiteNet Message reader
    /// Reads incoming messages and deserializes them into a message wrapper
    /// </summary>
    class LiteNetMessageReader
    {
        /// <summary>
        /// Serialization provider - used for serializing and deserializing messages
        /// </summary>
        private readonly ISerializationProvider _serializationProvider;
        
        /// <summary>
        /// Type map - contians list of types and hashes
        /// </summary>
        private readonly ITypeHashMap _typeHashMap;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly IFenrirLogger _logger;

        /// <summary>
        /// Object pool of byte stream readers - used for incoming messages
        /// </summary>
        private readonly RecyclableObjectPool<ByteStreamReader> _byteStreamReaderPool;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="serializationProvider">Serialization Provider</param>
        /// <param name="typeHashMap">Type Hash Map</param>
        /// <param name="logger">Logger</param>
        /// <param name="byteStreamReaderPool">Object pool of Byte Stream Readers</param>
        public LiteNetMessageReader(ISerializationProvider serializationProvider, ITypeHashMap typeHashMap, IFenrirLogger logger, RecyclableObjectPool<ByteStreamReader> byteStreamReaderPool)
        {
            _serializationProvider = serializationProvider;
            _typeHashMap = typeHashMap;
            _logger = logger;
            _byteStreamReaderPool = byteStreamReaderPool;
        }

        /// <summary>
        /// Reads an incoming message from byte stream (NetDataReader) and creates a message wrapper
        /// </summary>
        /// <param name="netDataReader">LiteNet NetDataReader</param>
        /// <param name="messageWrapper">Message Wrapper that will be written if message can be read</param>
        /// <returns>True if message could be read, false otherwise</returns>
        public bool TryReadMessage(NetDataReader netDataReader, out MessageWrapper messageWrapper)
        {
            // Message format: 
            // [8 bytes long message type hash]
            // [2 bytes ushort flags]
            //    [1 bit encrypted yes/no]
            //    [1 bit reserved]
            //    [1 bit reserved]
            //    [1 bit reserved]
            //    [12 bit request id]
            // [N bytes serialized message]

            messageWrapper = default(MessageWrapper);

            // 1. ulong Message type hash
            if (!netDataReader.TryGetULong(out ulong messageTypeHash))
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

            // 2. ushort (flags + request id)
            if(!netDataReader.TryGetUShort(out ushort flags))
            {
                _logger.Warning("Malformed message: no flags section");
                return false;
            }

            MessageFlags messageFlags = (MessageFlags)flags; // Flags enum

            ushort requestId = (ushort)(flags >> 4); // Request id

            // 3. byte[] Serialized message
            ByteStreamReader byteStreamReader = _byteStreamReaderPool.Get();
            byteStreamReader.SetNetDataReader(netDataReader);

            object data;

            try
            {
                data = _serializationProvider.Deserialize(dataType, byteStreamReader); 
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
            if(data is IEvent)
            {
                messageType = MessageType.Event;
            }
            else if(data is IRequest)
            {
                messageType = MessageType.Request;
            }
            else if(data is IResponse)
            {
                messageType = MessageType.Response;
            }
            else
            {
                _logger.Warning("Malformed message: unknown message type {0}, must be Event, Request or Response", dataType.Name);
                return false;
            }

            // Return message
            messageWrapper = new MessageWrapper()
            {
                MessageType = messageType,
                RequestId = requestId,
                MessageData = data,
                IsEncrypted = messageFlags.HasFlag(MessageFlags.Encrypted)
            };

            return true;
        }
    }
}
