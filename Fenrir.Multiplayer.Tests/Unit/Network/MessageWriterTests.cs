using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{

    [TestClass]
    public class MessageWriterTests
    {
        // Message format: 
        // 1. [8 bytes long message type hash]
        // 2. [1 byte flags]
        // 3. [1 byte channel number]
        // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
        // 5. [N bytes serialized message]

        [TestMethod]
        public void MessageWriter_WriteMessage_WritesEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapEvent(new TestEvent() { Value = "test" }, 123, MessageFlags.IsEncrypted, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            // Read hash
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestEvent>(), byteStreamReader.ReadULong()); // [ulong] type hash

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsFalse(flags.HasFlag(MessageFlags.HasRequestId));

            // Read channel id
            Assert.AreEqual(123, byteStreamReader.ReadByte());

            // Read data
            var testEvent = serializer.Deserialize<TestEvent>(byteStreamReader);

            Assert.AreEqual("test", testEvent.Value);
        }

        [TestMethod]
        public void MessageWriter_WriteMessage_WritesRequest()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapRequest(new TestRequest() { Value = "test" }, 456, 123, MessageFlags.IsEncrypted | MessageFlags.HasRequestId, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            // Read hash
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestRequest>(), byteStreamReader.ReadULong()); // [ulong] type hash

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsTrue(flags.HasFlag(MessageFlags.HasRequestId));

            // Read channel id
            Assert.AreEqual(123, byteStreamReader.ReadByte());

            // Read request id
            Assert.AreEqual(456, byteStreamReader.ReadShort());

            // Read data
            var testRequest = serializer.Deserialize<TestRequest>(byteStreamReader);

            Assert.AreEqual("test", testRequest.Value);
        }


        [TestMethod]
        public void MessageWriter_WriteMessage_WritesResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapResponse(new TestResponse() { Value = "test" }, 456, 123, MessageFlags.IsEncrypted | MessageFlags.HasRequestId, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            // Read hash
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            Assert.AreEqual(typeHashMap.GetTypeHash<TestResponse>(), byteStreamReader.ReadULong()); // [ulong] type hash

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsTrue(flags.HasFlag(MessageFlags.HasRequestId));

            // Read channel id
            Assert.AreEqual(123, byteStreamReader.ReadByte());

            // Read request id
            Assert.AreEqual(456, byteStreamReader.ReadShort());

            // Read data
            var testResponse = serializer.Deserialize<TestResponse>(byteStreamReader);

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
