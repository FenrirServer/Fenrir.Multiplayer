using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{
    [TestClass]
    public class MessageReaderTests
    {
        // Message format: 
        // 1. [1 byte message type + flags]
        // 2. [8 bytes long message type hash]
        // 3. [1 byte channel number]
        // 4. [2 bytes short requestId] - optional, if flags has HasRequestId
        // 5. [N bytes string debugInfo] - optional, if flags has IsDebug
        // 6. [N bytes serialized message]

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);

            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestEvent>()); // ulong type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            serializer.Serialize(new TestEvent() { Value = "test" }, byteStreamWriter); // byte[] data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Event, messageWrapper.MessageType);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestEvent));
            Assert.AreEqual("test", ((TestEvent)messageWrapper.MessageData).Value);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.IsTrue(messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsEvent_WhenEmptyData()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestEmptyEvent>()); // ulong type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            serializer.Serialize(new TestEmptyEvent(), byteStreamWriter); // byte[] data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Event, messageWrapper.MessageType);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestEmptyEvent));
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.IsTrue(messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsRequest()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Request;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestRequest>()); // [ulong] type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            serializer.Serialize(new TestRequest() { Value = "test" }, byteStreamWriter); // data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Request, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestRequest));
            Assert.AreEqual("test", ((TestRequest)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsRequestWithResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.RequestWithResponse;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestRequest>()); // [ulong] type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            byteStreamWriter.Write((short)456); // short Request id
            serializer.Serialize(new TestRequest() { Value = "test" }, byteStreamWriter); // data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.RequestWithResponse, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.AreEqual(456, messageWrapper.RequestId);
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestRequest));
            Assert.AreEqual("test", ((TestRequest)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Response;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestResponse>()); // [ulong] type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            byteStreamWriter.Write((short)456); // short Request id
            serializer.Serialize(new TestResponse() { Value = "test" }, byteStreamWriter); // byte[] data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Response, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.AreEqual(456, messageWrapper.RequestId);
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestResponse));
            Assert.AreEqual("test", ((TestResponse)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingFlags()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            // missing flags byte

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingMessageTypeHash()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            // no message type hash

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfInvalidMessageTypeHash()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write((ulong)123123); // invalid message type hash

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingChannelNumber()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write((ulong)typeHashMap.GetTypeHash<TestResponse>());
            // missing channel number byte

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingRequestId()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Request;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)MessageFlags.IsEncrypted);
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write((ulong)typeHashMap.GetTypeHash<TestResponse>());
            byteStreamWriter.Write((byte)123);
            // missing request id short, while HasRequestId flag is set

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsEvent_WithDebugFlag()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new NetworkSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byte typeAndFlagsCombined = (byte)MessageType.Event;
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined << 5);
            typeAndFlagsCombined = (byte)(typeAndFlagsCombined | (byte)(MessageFlags.IsEncrypted | MessageFlags.IsDebug));
            byteStreamWriter.Write(typeAndFlagsCombined); // byte type + flags
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestEvent>()); // ulong type hash
            byteStreamWriter.Write((byte)123); // byte Channel number
            byteStreamWriter.Write("test_debug_info_string");
            serializer.Serialize(new TestEvent() { Value = "test" }, byteStreamWriter); // byte[] data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Event, messageWrapper.MessageType);
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestEvent));
            Assert.AreEqual("test", ((TestEvent)messageWrapper.MessageData).Value);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.IsTrue(messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.IsTrue(messageWrapper.Flags.HasFlag(MessageFlags.IsDebug));
            Assert.AreEqual("test_debug_info_string", messageWrapper.DebugInfo);
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

        class TestEmptyEvent : IEvent, IByteStreamSerializable
        {
            public void Deserialize(IByteStreamReader reader)
            {
            }

            public void Serialize(IByteStreamWriter writer)
            {
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
