using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{

    [TestClass]
    public class MessageWriterTests
    {
        // Message format: 
        // 1. [1 byte flags]
        // 2. [8 bytes long message type hash]
        // 3. [1 byte channel number]
        // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
        // 5. [N bytes string debugInfo] - optional, if flags has IsDebug
        // 6. [N bytes serialized message]

        [TestMethod]
        public void MessageWriter_WriteMessage_WritesEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapEvent(new TestEvent() { Value = "test" }, 123, MessageFlags.IsEncrypted, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsFalse(flags.HasFlag(MessageFlags.HasRequestId));

            // Read hash
            Assert.AreEqual(typeHashMap.GetTypeHash<TestEvent>(), byteStreamReader.ReadULong()); // [ulong] type hash

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
            var serializer = new NetworkSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapRequest(new TestRequest() { Value = "test" }, 456, 123, MessageFlags.IsEncrypted | MessageFlags.HasRequestId, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsTrue(flags.HasFlag(MessageFlags.HasRequestId));

            // Read hash
            Assert.AreEqual(typeHashMap.GetTypeHash<TestRequest>(), byteStreamReader.ReadULong()); // [ulong] type hash


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
            var serializer = new NetworkSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapResponse(new TestResponse() { Value = "test" }, 456, 123, MessageFlags.IsEncrypted | MessageFlags.HasRequestId, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsTrue(flags.HasFlag(MessageFlags.HasRequestId));

            // Read hash
            Assert.AreEqual(typeHashMap.GetTypeHash<TestResponse>(), byteStreamReader.ReadULong()); // [ulong] type hash

            // Read channel id
            Assert.AreEqual(123, byteStreamReader.ReadByte());

            // Read request id
            Assert.AreEqual(456, byteStreamReader.ReadShort());

            // Read data
            var testResponse = serializer.Deserialize<TestResponse>(byteStreamReader);

            Assert.AreEqual("test", testResponse.Value);
        }


        [TestMethod]
        public void MessageWriter_WriteMessage_WritesDebugInfo_WhenIsDebugFlagSet()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageWriter = new MessageWriter(serializer, typeHashMap, new EventBasedLogger());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var messageWrapper = MessageWrapper.WrapEvent(new TestEvent() { Value = "test" }, 123, MessageFlags.IsEncrypted | MessageFlags.IsDebug, MessageDeliveryMethod.ReliableOrdered);

            messageWriter.WriteMessage(byteStreamWriter, messageWrapper);

            // Validate 
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);

            // Read flags
            MessageFlags flags = (MessageFlags)byteStreamReader.ReadByte();
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsFalse(flags.HasFlag(MessageFlags.HasRequestId));
            Assert.IsTrue(flags.HasFlag(MessageFlags.IsDebug));

            // Read hash
            Assert.AreEqual(typeHashMap.GetTypeHash<TestEvent>(), byteStreamReader.ReadULong()); // [ulong] type hash

            // Read channel id
            Assert.AreEqual(123, byteStreamReader.ReadByte());

            // Read debug info
            string debugInfo = byteStreamReader.ReadString();
            Assert.IsNotNull(debugInfo);
            Assert.IsTrue(debugInfo.Contains("TestEvent")); // Contains message type that is normally not sent over the wire

            // Read data
            var testEvent = serializer.Deserialize<TestEvent>(byteStreamReader);

            Assert.AreEqual("test", testEvent.Value);
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
