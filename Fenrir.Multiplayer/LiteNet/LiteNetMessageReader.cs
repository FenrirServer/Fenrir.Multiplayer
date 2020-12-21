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
        private readonly ITypeMap _typeMap;

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
        /// <param name="typeMap">Type Map</param>
        /// <param name="logger">Logger</param>
        /// <param name="byteStreamReaderPool">Object pool of Byte Stream Readers</param>
        public LiteNetMessageReader(ISerializationProvider serializationProvider, ITypeMap typeMap, IFenrirLogger logger, RecyclableObjectPool<ByteStreamReader> byteStreamReaderPool)
        {
            _serializationProvider = serializationProvider;
            _typeMap = typeMap;
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
            messageWrapper = default(MessageWrapper);

            // 1. [byte] Type of the message
            if (!netDataReader.TryGetByte(out byte messageTypeByte))
            {
                _logger.Warning("Malformed message: no message type [byte]");
                return false;
            }
            if(!Enum.IsDefined(typeof(MessageType), messageTypeByte))
            {
                _logger.Warning("Malformed message: failed {0} is not a valid {1}", messageTypeByte, nameof(MessageType));
                return false;
            }

            MessageType messageType = (MessageType)messageTypeByte;

            // 2. [int] Request Id (if message type is request)
            int requestId = 0;
            if(messageType == MessageType.Request || messageType == MessageType.Response)
            {
                if(!netDataReader.TryGetInt(out requestId))
                {
                    _logger.Warning("Malformed message: no request id [int]");
                    return false;
                }
            }

            // 3. [ulong] Message type hash
            if(!netDataReader.TryGetULong(out ulong messageTypeHash))
            {
                _logger.Warning("Malformed message: no message type hash [long]");
                return false;
            }

            // Find message type
            if(!_typeMap.TryGetTypeByHash(messageTypeHash, out Type dataType))
            {
                _logger.Warning("Malformed message: no message type with hash {0} in type map", messageTypeHash);
                return false;
            }

            // 4. [byte[]] Serialized message
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

            // Return message
            messageWrapper = new MessageWrapper()
            {
                MessageType = messageType,
                RequestId = requestId,
                MessageData = data
            };

            return true;
        }
    }
}
