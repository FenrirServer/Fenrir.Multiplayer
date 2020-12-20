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
        public void SerializationProvider_SerializesWithByteStreamSerializable()
        {
            var test = new TestSerializable() { TestString = "test", TestInteger = 123 };
            var serializationProvider = new SerializationProvider();

            var byteStreamWriter = new ByteStreamWriter();
            serializationProvider.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes);
            TestSerializable test2 = serializationProvider.Deserialize<TestSerializable>(byteStreamReader);

            Assert.AreEqual(test.TestString, test2.TestString);
            Assert.AreEqual(test.TestInteger, test2.TestInteger);
        }


        [TestMethod]
        public void SerializationProvider_SerializesWithNestedByteStreamSerializable()
        {
            var test = new TestNestedSerializable()
            {
                Test = new TestSerializable() { TestString = "test", TestInteger = 123 }
            };

            var serializationProvider = new SerializationProvider();

            var byteStreamWriter = new ByteStreamWriter();
            serializationProvider.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes);
            TestNestedSerializable test2 = serializationProvider.Deserialize<TestNestedSerializable>(byteStreamReader);

            Assert.IsNotNull(test.Test);
            Assert.IsNotNull(test2.Test);
            Assert.AreEqual(test.Test.TestString, test2.Test.TestString);
            Assert.AreEqual(test.Test.TestInteger, test2.Test.TestInteger);
        }

        [TestMethod]
        public void SerializationProvider_SerializesWithCustomContractSerializer()
        {
            var test = new TestDataContract() { TestString = "test", TestInteger = 123 };
            var serializationProvider = new SerializationProvider();
            serializationProvider.SetContractSerializer(new TestContractSerializer());

            var byteStreamWriter = new ByteStreamWriter();
            serializationProvider.Serialize(test, byteStreamWriter);

            var byteStreamReader = new ByteStreamReader(byteStreamWriter.Bytes);
            TestDataContract test2 = serializationProvider.Deserialize<TestDataContract>(byteStreamReader);

            Assert.AreEqual(test.TestString, test2.TestString);
            Assert.AreEqual(test.TestInteger, test2.TestInteger);
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

        class TestContractSerializer : IContractSerializer
        {
            public TData Deserialize<TData>(IByteStreamReader byteStreamReader) where TData : new()
            {
                byte[] bytes = byteStreamReader.ReadBytesWithLength();
                using var memoryStream = new MemoryStream(bytes);

                var dataContractSerializer = new DataContractSerializer(typeof(TData));
                TData data = (TData)dataContractSerializer.ReadObject(memoryStream);
                return data;
            }

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

        [DataContract]
        class TestDataContract
        {
            [DataMember]
            public string TestString;

            [DataMember]
            public int TestInteger;
        }



        #endregion
    }
}
