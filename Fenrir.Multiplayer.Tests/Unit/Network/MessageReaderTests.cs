﻿using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using LiteNetLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.LiteNetProtocol
{
    [TestClass]
    public class MessageReaderTests
    {
        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsEvent()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestEvent>()); // ulong type hash
            byteStreamWriter.Write((byte)MessageFlags.IsEncrypted); // byte flags
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
            Assert.IsFalse(messageWrapper.Flags.HasFlag(MessageFlags.HasRequestId));
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsEvent_WhenEmptyData()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestEmptyEvent>()); // ulong type hash
            byteStreamWriter.Write((byte)MessageFlags.IsEncrypted); // byte flags
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
            Assert.IsFalse(messageWrapper.Flags.HasFlag(MessageFlags.HasRequestId));
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsRequest()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestRequest>()); // [ulong] type hash
            byteStreamWriter.Write((byte)(MessageFlags.IsEncrypted | MessageFlags.HasRequestId)); // byte flags
            byteStreamWriter.Write((byte)123); // byte Channel number
            byteStreamWriter.Write((short)456); // short Request id
            serializer.Serialize(new TestRequest() { Value = "test" }, byteStreamWriter); // data

            // Read message
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);

            Assert.IsTrue(result);
            Assert.AreEqual(MessageType.Request, messageWrapper.MessageType);
            Assert.AreEqual(123, messageWrapper.Channel);
            Assert.AreEqual(456, messageWrapper.RequestId);
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.IsEncrypted));
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.HasRequestId));
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestRequest));
            Assert.AreEqual("test", ((TestRequest)messageWrapper.MessageData).Value);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReadsResponse()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(typeHashMap.GetTypeHash<TestResponse>()); // [ulong] type hash
            byteStreamWriter.Write((byte)(MessageFlags.IsEncrypted | MessageFlags.HasRequestId)); // byte flags
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
            Assert.AreEqual(true, messageWrapper.Flags.HasFlag(MessageFlags.HasRequestId));
            Assert.IsInstanceOfType(messageWrapper.MessageData, typeof(TestResponse));
            Assert.AreEqual("test", ((TestResponse)messageWrapper.MessageData).Value);
        }


        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingMessageTypeHash()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // empty data
            var byteStreamReader = new ByteStreamReader(serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfInvalidMessageTypeHash()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write((ulong)123123); // invalid message type hash

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingFlags()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write((ulong)typeHashMap.GetTypeHash<TestResponse>());
            // missing flags byte

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingChannelNumber()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write((ulong)typeHashMap.GetTypeHash<TestResponse>());
            byteStreamWriter.Write((byte)(MessageFlags.IsEncrypted | MessageFlags.HasRequestId));
            // missing channel number byte

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MessageReader_TryReadMessage_ReturnsFalse_IfMissingRequestId()
        {
            var typeHashMap = new TypeHashMap();
            var serializer = new FenrirSerializer();
            var messageReader = new MessageReader(serializer, typeHashMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamReader>(() => new ByteStreamReader(serializer)));

            // Write test data
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write((ulong)typeHashMap.GetTypeHash<TestResponse>());
            byteStreamWriter.Write((byte)(MessageFlags.IsEncrypted | MessageFlags.HasRequestId));
            byteStreamWriter.Write((byte)123);
            // missing request id short, while HasRequestId flag is set

            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            bool result = messageReader.TryReadMessage(byteStreamReader, out MessageWrapper messageWrapper);
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
