using Fenrir.Multiplayer.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void Serializer_SerializesWithByteStreamSerializable()
        {
            var test = new TestSerializable() { TestString = "test", TestInteger = 123 };
            var serializer = new FenrirSerializer();

            var byteStreamWriter = new ByteStreamWriter(serializer);
            serializer.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes, serializer);
            TestSerializable test2 = serializer.Deserialize<TestSerializable>(byteStreamReader);

            Assert.AreEqual(test.TestString, test2.TestString);
            Assert.AreEqual(test.TestInteger, test2.TestInteger);
        }


        [TestMethod]
        public void Serializer_SerializesWithNestedByteStreamSerializable()
        {
            var test = new TestNestedSerializable()
            {
                Test = new TestSerializable() { TestString = "test", TestInteger = 123 }
            };

            var serializer = new FenrirSerializer();

            var byteStreamWriter = new ByteStreamWriter(serializer);
            serializer.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes, serializer);
            TestNestedSerializable test2 = serializer.Deserialize<TestNestedSerializable>(byteStreamReader);

            Assert.IsNotNull(test.Test);
            Assert.IsNotNull(test2.Test);
            Assert.AreEqual(test.Test.TestString, test2.Test.TestString);
            Assert.AreEqual(test.Test.TestInteger, test2.Test.TestInteger);
        }

        [TestMethod]
        public void Serializer_SerializesWithCustomTypeSerializer()
        {
            var test = new TestDataContract() { TestString = "test", TestInteger = 123 };
            var serializer = new FenrirSerializer();
            serializer.SetTypeSerializer(new TestContractSerializer());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            serializer.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes, serializer);
            TestDataContract test2 = serializer.Deserialize<TestDataContract>(byteStreamReader);

            Assert.AreEqual(test.TestString, test2.TestString);
            Assert.AreEqual(test.TestInteger, test2.TestInteger);
        }

        [TestMethod]
        public void Serializer_SerializesWithCustomTypeSerializer_ForGivenType()
        {
            var test = new TestDataContract() { TestString = "test", TestInteger = 123 };
            var serializer = new FenrirSerializer();
            serializer.AddTypeSerializer<TestDataContract>(new TestContractTypeSerializer());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            serializer.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes, serializer);
            TestDataContract test2 = serializer.Deserialize<TestDataContract>(byteStreamReader);

            Assert.AreEqual(test.TestString, test2.TestString);
            Assert.AreEqual(test.TestInteger, test2.TestInteger);
        }

        [TestMethod]
        public void Serializer_SerializesWithCustomTypeSerializer_ForGivenType_WithNested()
        {
            var serializer = new FenrirSerializer();
            serializer.AddTypeSerializer<LinkedListNode>(new LinkedListNodeSerializer());

            LinkedListNode node1 = new LinkedListNode() { Value = "node1" };
            LinkedListNode node2 = new LinkedListNode() { Value = "node2" };
            node1.Next = node2;

            var byteStreamWriter = new ByteStreamWriter(serializer);
            serializer.Serialize(node1, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes, serializer);
            LinkedListNode test1Deserialized = serializer.Deserialize<LinkedListNode>(byteStreamReader);

            Assert.AreEqual(node1.Value, test1Deserialized.Value);
            Assert.AreEqual(node1.Next.Value, test1Deserialized.Next.Value);
        }


        [TestMethod]
        public void Serializer_Serialize_ThrowsSerializationException_WhenByteStreamSerializableThrows()
        {
            var test = new TestThrowingSerializable();
            var serializer = new FenrirSerializer();

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Serialize(test, byteStreamWriter));
            Assert.IsInstanceOfType(e.InnerException, typeof(InvalidOperationException));
            Assert.AreEqual("test", e.InnerException.Message);
        }


        [TestMethod]
        public void Serializer_Deserialize_ThrowsSerializationException_WhenByteStreamSerializableThrows()
        {
            var serializer = new FenrirSerializer();
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(true); // To make sure there is some data
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Deserialize<TestThrowingSerializable>(byteStreamReader));
            Assert.IsInstanceOfType(e.InnerException, typeof(InvalidOperationException));
            Assert.AreEqual("test", e.InnerException.Message);
        }

        [TestMethod]
        public void Serializer_Serialize_ThrowsSerializationException_WhenTypeSerializerThrows()
        {
            var test = new TestDataContract();
            var serializer = new FenrirSerializer();
            serializer.SetTypeSerializer(new ThrowingContractSerializer());

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Serialize(test, byteStreamWriter));
            Assert.IsInstanceOfType(e.InnerException, typeof(InvalidOperationException));
            Assert.AreEqual("test", e.InnerException.Message);
        }

        [TestMethod]
        public void Serializer_Deserialize_ThrowsSerializationException_WhenTypeSerializerThrows()
        {
            var serializer = new FenrirSerializer();
            serializer.SetTypeSerializer(new ThrowingContractSerializer());
            var byteStreamWriter = new ByteStreamWriter(serializer);
            byteStreamWriter.Write(true); // To make sure there is some data
            var byteStreamReader = new ByteStreamReader(byteStreamWriter, serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Deserialize<TestDataContract>(byteStreamReader));
            Assert.IsInstanceOfType(e.InnerException, typeof(InvalidOperationException));
            Assert.AreEqual("test", e.InnerException.Message);
        }
        
        [TestMethod]
        public void Serializer_Deserialize_ThrowsSerializationException_WhenEndOfStreamReached()
        {
            var serializer = new FenrirSerializer();
            serializer.SetTypeSerializer(new ThrowingContractSerializer());
            var byteStreamReader = new ByteStreamReader(serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Deserialize<TestDataContract>(byteStreamReader));
        }

        [TestMethod]
        public void Serializer_Serialize_ThrowsSerializationException_WhenCircularReferenceReachesMaxDepth()
        {
            var serializer = new FenrirSerializer();
            serializer.AddTypeSerializer<LinkedListNode>(new LinkedListNodeSerializer());

            LinkedListNode test1 = new LinkedListNode();
            LinkedListNode test2 = new LinkedListNode();
            test1.Next = test2;
            test2.Next = test1; // Circular reference

            var byteStreamWriter = new ByteStreamWriter(serializer);
            var e = Assert.ThrowsException<SerializationException>(() => serializer.Serialize(test1, byteStreamWriter));
        }

        #region Test Fixtures
        class TestSerializable : IByteStreamSerializable
        {
            public string TestString;
            public int TestInteger; 

            public void Deserialize(IByteStreamReader reader)
            {
                TestString = reader.ReadString();
                TestInteger = reader.ReadInt();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(TestString);
                writer.Write(TestInteger);
            }
        }

        class TestThrowingSerializable : IByteStreamSerializable
        {
            public void Deserialize(IByteStreamReader reader)
            {
                throw new InvalidOperationException("test");
            }

            public void Serialize(IByteStreamWriter writer)
            {
                throw new InvalidOperationException("test");
            }
        }


        class TestNestedSerializable : IByteStreamSerializable
        {
            public TestSerializable Test;

            public void Deserialize(IByteStreamReader reader)
            {
                Test = reader.Read<TestSerializable>();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Test);
            }
        }

        class TestContractSerializer : ITypeSerializer
        {
            public object Deserialize(Type type, IByteStreamReader byteStreamReader)
            {
                byte[] bytes = byteStreamReader.ReadBytesWithLength();
                using var memoryStream = new MemoryStream(bytes);

                var dataContractSerializer = new DataContractSerializer(type);
                object data = dataContractSerializer.ReadObject(memoryStream);
                return data;
            }

            public void Serialize(object data, IByteStreamWriter byteStreamWriter)
            {
                using var memoryStream = new MemoryStream();

                var dataContractSerializer = new DataContractSerializer(data.GetType());
                dataContractSerializer.WriteObject(memoryStream, data);

                byte[] objectBytes = memoryStream.ToArray();
                byteStreamWriter.WriteBytesWithLength(objectBytes);
            }
        }

        class ThrowingContractSerializer : ITypeSerializer
        {
            public object Deserialize(Type type, IByteStreamReader byteStreamReader)
            {
                throw new InvalidOperationException("test");
            }

            public void Serialize(object data, IByteStreamWriter byteStreamWriter)
            {
                throw new InvalidOperationException("test");
            }
        }

        class TestContractTypeSerializer : ITypeSerializer<TestDataContract>
        {
            public TestDataContract Deserialize(IByteStreamReader byteStreamReader)
            {
                TestDataContract contract = new TestDataContract();
                contract.TestString = byteStreamReader.ReadString();
                contract.TestInteger = byteStreamReader.ReadInt();
                return contract;
            }

            public void Serialize(TestDataContract data, IByteStreamWriter byteStreamWriter)
            {
                byteStreamWriter.Write(data.TestString);
                byteStreamWriter.Write(data.TestInteger);
            }
        }


        class LinkedListNodeSerializer : ITypeSerializer<LinkedListNode>
        {
            public LinkedListNode Deserialize(IByteStreamReader byteStreamReader)
            {
                LinkedListNode contract = new LinkedListNode();
                contract.Value = byteStreamReader.ReadString();
                contract.Next = byteStreamReader.Read<LinkedListNode>();
                return contract;
            }

            public void Serialize(LinkedListNode data, IByteStreamWriter byteStreamWriter)
            {
                byteStreamWriter.Write(data.Value);
                byteStreamWriter.Write(data.Next);
            }
        }


        [DataContract]
        class TestDataContract
        {
            [DataMember]
            public string TestString;

            [DataMember]
            public int TestInteger;
        }


        class LinkedListNode
        {
            public string Value;

            public LinkedListNode Next;
        }

        #endregion
    }
}
