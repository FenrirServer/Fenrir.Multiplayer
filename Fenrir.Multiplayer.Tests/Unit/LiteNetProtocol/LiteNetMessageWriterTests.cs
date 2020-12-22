using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{

    [TestClass]
    public class LiteNetMessageWriterTests
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

        [TestMethod]
        public void LiteNetMessageWriter_WriteMessage_WritesEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageWriter = new LiteNetMessageWriter(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamWriter>());

            var netDataWriter = new NetDataWriter();
            var messageWrapper = new MessageWrapper() { MessageType = MessageType.Event, MessageData = new TestEvent() { Value = "test" }, IsEncrypted = true };
            messageWriter.WriteMessage(netDataWriter, messageWrapper);

            // Validate 
            // Get hash
            var netDataReader = new NetDataReader(netDataWriter.Data);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestEvent>(), netDataReader.GetULong()); // [ulong] type hash
            
            // Get flags
            ushort flags = netDataReader.GetUShort();
            MessageFlags messageFlags = (MessageFlags)flags;
            Assert.IsTrue(messageFlags.HasFlag(MessageFlags.Encrypted));

            var testEvent = serializationProvider.Deserialize<TestEvent>(new ByteStreamReader(netDataReader));

            Assert.AreEqual("test", testEvent.Value);
        }

        [TestMethod]
        public void LiteNetMessageWriter_WriteMessage_WritesRequest()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageWriter = new LiteNetMessageWriter(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamWriter>());

            var netDataWriter = new NetDataWriter();
            var messageWrapper = new MessageWrapper() { MessageType = MessageType.Request, RequestId = 123, MessageData = new TestRequest() { Value = "test" }, IsEncrypted = true };
            messageWriter.WriteMessage(netDataWriter, messageWrapper);

            // Validate 
            // Get hash
            var netDataReader = new NetDataReader(netDataWriter.Data);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestRequest>(), netDataReader.GetULong()); // [ulong] type hash

            // Get flags
            ushort flags = netDataReader.GetUShort();
            MessageFlags messageFlags = (MessageFlags)flags; // Flags enum
            ushort requestId = (ushort)(flags >> 4); // Request id

            Assert.IsTrue(messageFlags.HasFlag(MessageFlags.Encrypted));
            Assert.AreEqual(123, requestId);

            var testRequest = serializationProvider.Deserialize<TestRequest>(new ByteStreamReader(netDataReader));

            Assert.AreEqual("test", testRequest.Value);
        }


        [TestMethod]
        public void LiteNetMessageWriter_WriteMessage_WritesResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializationProvider = new SerializationProvider();
            var messageWriter = new LiteNetMessageWriter(serializationProvider, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamWriter>());

            var netDataWriter = new NetDataWriter();
            var messageWrapper = new MessageWrapper() { MessageType = MessageType.Response, RequestId = 123, MessageData = new TestResponse() { Value = "test" }, IsEncrypted = true };
            messageWriter.WriteMessage(netDataWriter, messageWrapper);

            // Validate 
            // Get hash
            var netDataReader = new NetDataReader(netDataWriter.Data);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestResponse>(), netDataReader.GetULong()); // [ulong] type hash

            // Get flags
            ushort flags = netDataReader.GetUShort();
            MessageFlags messageFlags = (MessageFlags)flags; // Flags enum
            ushort requestId = (ushort)(flags >> 4); // Request id

            Assert.IsTrue(messageFlags.HasFlag(MessageFlags.Encrypted));
            Assert.AreEqual(123, requestId);

            var testResponse = serializationProvider.Deserialize<TestResponse>(new ByteStreamReader(netDataReader));

            Assert.AreEqual("test", testResponse.Value);
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
