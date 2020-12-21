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
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put((byte)MessageType.Event); // [byte] message type
            netDataWriter.Put(typeMap.GetTypeHash<TestEvent>()); // [ulong] type hash
            serializationProvider.Serialize(new TestEvent() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Event, messageWrapper.MessageType);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestEvent));
            Assert.AreEqual("test", ((TestEvent)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReadsRequest()
        {
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put((byte)MessageType.Request); // [byte] message type
            netDataWriter.Put(1); // Request id
            netDataWriter.Put(typeMap.GetTypeHash<TestRequest>()); // [ulong] type hash
            serializationProvider.Serialize(new TestRequest() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Request, messageWrapper.MessageType);
            Assert.AreEqual(1, messageWrapper.RequestId);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestRequest));
            Assert.AreEqual("test", ((TestRequest)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReadsResponse()
        {
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put((byte)MessageType.Response); // [byte] message type
            netDataWriter.Put(1); // Request id
            netDataWriter.Put(typeMap.GetTypeHash<TestResponse>()); // [ulong] type hash
            serializationProvider.Serialize(new TestResponse() { Value = "test" }, new ByteStreamWriter(netDataWriter)); // data

            // Read message
            var netDataReader = new NetDataReader(netDataWriter.Data);
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Response, messageWrapper.MessageType);
            Assert.AreEqual(1, messageWrapper.RequestId);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestResponse));
            Assert.AreEqual("test", ((TestResponse)messageWrapper.MessageData).Value);
        }


        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfMissingMessageType()
        {
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

            // Write test data
            var netDataReader = new NetDataReader();
            bool result = messageReader.TryReadMessage(netDataReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void LiteNetMessageReader_TryReadMessage_ReturnsFalse_IfInvalidMessageType()
        {
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

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
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

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
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageReader = new LiteNetMessageReader(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>());

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
