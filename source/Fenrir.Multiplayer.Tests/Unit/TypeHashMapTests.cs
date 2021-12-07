using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Fenrir.Multiplayer.Tests.Unit
{
    [TestClass]
    public class TypeHashMapTests
    {
        [TestMethod]
        public void TypeHashMap_GetTypeHash_CalculatesDeterministicHash()
        {
            // Simple check to ensure type hash is deterministic for multiple instances of typeHashMap

            var typeHashMap1 = new TypeHashMap();
            var typeHashMap2 = new TypeHashMap();

            Assert.AreEqual(typeHashMap1.GetTypeHash<byte>(), typeHashMap2.GetTypeHash<byte>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<sbyte>(), typeHashMap2.GetTypeHash<sbyte>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<char>(), typeHashMap2.GetTypeHash<char>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<int>(), typeHashMap2.GetTypeHash<int>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<uint>(), typeHashMap2.GetTypeHash<uint>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<long>(), typeHashMap2.GetTypeHash<long>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<ulong>(), typeHashMap2.GetTypeHash<ulong>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<float>(), typeHashMap2.GetTypeHash<float>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<double>(), typeHashMap2.GetTypeHash<double>());
            Assert.AreEqual(typeHashMap1.GetTypeHash<string>(), typeHashMap2.GetTypeHash<string>());
        }


        [TestMethod]
        public void TypeHashMap_AddType_AddsType()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));

            ulong hash = typeHashMap.GetTypeHash<int>();
            Type type = typeHashMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeHashMap_AddTypeGeneric_AddsType()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType<int>();

            ulong hash = typeHashMap.GetTypeHash(typeof(int));
            Type type = typeHashMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeHashMap_GetTypeHash_AutomaticallyAddsHash()
        {
            var typeHashMap = new TypeHashMap();
            ulong hash = typeHashMap.GetTypeHash<int>();
            Type type = typeHashMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }

        [TestMethod]
        public void TypeHashMap_GetTypeHash_ReturnsHash()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));

            ulong hash = typeHashMap.GetTypeHash<int>();
            Type type = typeHashMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeHashMap_GetTypeByHash_ReturnsType()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));

            ulong hash = typeHashMap.GetTypeHash<int>();
            typeHashMap.TryGetTypeByHash(hash, out Type type);

            Assert.AreEqual(type, typeof(int));
        }
        

        [TestMethod]
        public void TypeHashMap_TryGetTypeByHash_ReturnsTrue()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));

            ulong hash = typeHashMap.GetTypeHash<int>();
            bool result = typeHashMap.TryGetTypeByHash(hash, out Type type);
            Assert.IsTrue(result);

            Assert.AreEqual(type, typeof(int));
        }

        [TestMethod]
        public void TypeHashMap_TryGetTypeByHash_ReturnsFalse_WhenNoType()
        {
            var typeHashMap = new TypeHashMap();
            bool result = typeHashMap.TryGetTypeByHash(12340000, out Type type);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public void TypeHashMap_RemoveType_RemovesType()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType<int>();
            ulong hash = typeHashMap.GetTypeHash<int>();

            typeHashMap.RemoveType(typeof(int));

            Assert.ThrowsException<TypeHashMapException>(() => typeHashMap.GetTypeByHash(hash));
        }


        [TestMethod]
        public void TypeHashMap_RemoveTypeGeneric_RemovesType()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType<int>();
            ulong hash = typeHashMap.GetTypeHash<int>();

            typeHashMap.RemoveType<int>();

            Assert.ThrowsException<TypeHashMapException>(() => typeHashMap.GetTypeByHash(hash));
        }

        [TestMethod]
        public void TypeHashMap_HasType_ReturnsTrue_WhenTypeAdded()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));
            Assert.IsTrue(typeHashMap.HasTypeHash<int>());
        }

        [TestMethod]
        public void TypeHashMap_HasType_ReturnsTrue_WhenTypeRemoved()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType(typeof(int));
            typeHashMap.RemoveType(typeof(int));
            Assert.IsFalse(typeHashMap.HasTypeHash<int>());
        }

        [TestMethod]
        public void TypeHashMap_HasTypeGeneric_ReturnsTrue_WhenTypeAdded()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType<int>();
            Assert.IsTrue(typeHashMap.HasTypeHash<int>());
        }

        [TestMethod]
        public void TypeHashMap_HasTypeGeneric_ReturnsFalse_WhenTypeRemoved()
        {
            var typeHashMap = new TypeHashMap();
            typeHashMap.AddType<int>();
            typeHashMap.RemoveType<int>();
            Assert.IsFalse(typeHashMap.HasTypeHash<int>());
        }
    }
}
