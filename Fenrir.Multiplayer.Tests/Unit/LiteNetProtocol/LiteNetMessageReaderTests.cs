using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{
    [TestClass]
    public class LiteNetMessageReaderTests
    {
        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReadsEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(typeHashMap.GetTypeHash<TestEvent>()); // [ulong] type hash
            netDataWriter.Put((ushort)MessageFlags.Encrypted); // [ushort] flags
            serializationProvider.Serialize(new TestEvent() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Event, messageWrapper.MessageType);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestEvent));
            Assert.AreEqual("test", ((TestEvent)messageWrapper.MessageData).Value);
            Assert.AreEqual(true, messageWrapper.IsEncrypted);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReadsRequest()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(typeHashMap.GetTypeHash<TestRequest>()); // [ulong] type hash

            ushort requestId = 123;
            ushort flags = 0;
            flags |= (ushort)(requestId << 4);
            flags |= (ushort)MessageFlags.Encrypted;
            netDataWriter.Put(flags); // [ushort] flags

            serializationProvider.Serialize(new TestRequest() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Request, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.RequestId);
            Assert.AreEqual(true, messageWrapper.IsEncrypted);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestRequest));
            Assert.AreEqual("test", ((TestRequest)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReadsResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(typeHashMap.GetTypeHash<TestResponse>()); // [ulong] type hash

            ushort requestId = 123;
            ushort flags = 0;
            flags |= (ushort)(requestId << 4);
            flags |= (ushort)MessageFlags.Encrypted;
            netDataWriter.Put(flags); // [ushort] flags

            serializationProvider.Serialize(new TestResponse() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // byte[] data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Response, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.RequestId);
            Assert.AreEqual(true, messageWrapper.IsEncrypted);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestResponse));
            Assert.AreEqual("test", ((TestResponse)messageWrapper.MessageData).Value);
        }


        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfMissingMessageType()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataReader = new NetDataReader();
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfInvalidMessageType()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(123); // invalid message type

            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfInvalidRequestId()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put((byte)MessageType.Request); // [byte] message type

            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfInvalidTypeHash()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put((byte)MessageType.Request); // [byte] message type
            netDataWriter.Put((ulong)12340000); // [byte] message type

            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        #region Test Fixtures
        class TestEvent : IEvent, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }

        class TestRequest : IRequest, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }

        class TestResponse : IResponse, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }
        #endregion
    }
}
