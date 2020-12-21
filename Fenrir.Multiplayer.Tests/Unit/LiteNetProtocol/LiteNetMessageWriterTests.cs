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
        [TestMethod]
        public void LiteNetMessageWriter_WriteMessage_WritesEvent()
        {
            var typeMap = new TypeMap();
            var serializationProvider = new SerializationProvider();
            var messageWriter = new LiteNetMessageWriter(serializationProvider, typeMap, new EventBasedLogger(), new RecyclableObjectPool<ByteStreamWriter>());

            var netDataWriter = new NetDataWriter();
            var messageWrapper = new MessageWrapper() { MessageType = MessageType.Event, MessageData = new TestEvent() { Value = "test" } };
            messageWriter.WriteMessage(netDataWriter, messageWrapper);

            // Validate
            var netDataReader = new NetDataReader(netDataWriter.Data);
            Assert.AreEqual(MessageType.Event, (MessageType)netDataReader.GetByte()); // [byte] message type
            Assert.AreEqual(typeMap.GetTypeHash<TestEvent>(), netDataReader.GetULong()); // [ulong] type hash

            var testEvent = serializationProvider.Deserialize<TestEvent>(new ByteStreamReader(netDataReader));

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
