using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit
{
    [TestClass]
    public class TypeMapTests
    {
        [TestMethod]
        public void TypeMap_GetTypeHash_CalculatesDeterministicHash()
        {
            // Simple check to ensure type hash is deterministic for multiple instances of TypeMap

            var typeMap1 = new TypeMap();
            var typeMap2 = new TypeMap();

            Assert.AreEqual(typeMap1.GetTypeHash<byte>(), typeMap2.GetTypeHash<byte>());
            Assert.AreEqual(typeMap1.GetTypeHash<sbyte>(), typeMap2.GetTypeHash<sbyte>());
            Assert.AreEqual(typeMap1.GetTypeHash<char>(), typeMap2.GetTypeHash<char>());
            Assert.AreEqual(typeMap1.GetTypeHash<int>(), typeMap2.GetTypeHash<int>());
            Assert.AreEqual(typeMap1.GetTypeHash<uint>(), typeMap2.GetTypeHash<uint>());
            Assert.AreEqual(typeMap1.GetTypeHash<long>(), typeMap2.GetTypeHash<long>());
            Assert.AreEqual(typeMap1.GetTypeHash<ulong>(), typeMap2.GetTypeHash<ulong>());
            Assert.AreEqual(typeMap1.GetTypeHash<float>(), typeMap2.GetTypeHash<float>());
            Assert.AreEqual(typeMap1.GetTypeHash<double>(), typeMap2.GetTypeHash<double>());
            Assert.AreEqual(typeMap1.GetTypeHash<string>(), typeMap2.GetTypeHash<string>());
        }


        [TestMethod]
        public void TypeMap_AddType_AddsType()
        {
            var typeMap = new TypeMap();
            typeMap.AddType(typeof(int));

            ulong hash = typeMap.GetTypeHash<int>();
            Type type = typeMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeMap_AddTypeGeneric_AddsType()
        {
            var typeMap = new TypeMap();
            typeMap.AddType<int>();

            ulong hash = typeMap.GetTypeHash(typeof(int));
            Type type = typeMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeMap_GetTypeHash_AutomaticallyAddsHash()
        {
            var typeMap = new TypeMap();
            ulong hash = typeMap.GetTypeHash<int>();
            Type type = typeMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }

        [TestMethod]
        public void TypeMap_GetTypeHash_ReturnsHash()
        {
            var typeMap = new TypeMap();
            typeMap.AddType(typeof(int));

            ulong hash = typeMap.GetTypeHash<int>();
            Type type = typeMap.GetTypeByHash(hash);

            Assert.AreEqual(type, typeof(int));
        }


        [TestMethod]
        public void TypeMap_GetTypeByHash_ReturnsType()
        {
            var typeMap = new TypeMap();
            typeMap.AddType(typeof(int));

            ulong hash = typeMap.GetTypeHash<int>();
            typeMap.TryGetTypeByHash(hash, out Type type);

            Assert.AreEqual(type, typeof(int));
        }
        

        [TestMethod]
        public void TypeMap_TryGetTypeByHash_ReturnsTrue()
        {
            var typeMap = new TypeMap();
            typeMap.AddType(typeof(int));

            ulong hash = typeMap.GetTypeHash<int>();
            bool result = typeMap.TryGetTypeByHash(hash, out Type type);
            Assert.IsTrue(result);

            Assert.AreEqual(type, typeof(int));
        }

        [TestMethod]
        public void TypeMap_TryGetTypeByHash_ReturnsFalse_WhenNoType()
        {
            var typeMap = new TypeMap();
            bool result = typeMap.TryGetTypeByHash(12340000, out Type type);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public void TypeMap_RemoveType_RemovesType()
        {
            var typeMap = new TypeMap();
            typeMap.AddType<int>();
            ulong hash = typeMap.GetTypeHash<int>();

            typeMap.RemoveType(typeof(int));

            Assert.ThrowsException<TypeMapException>(() => typeMap.GetTypeByHash(hash));
        }


        [TestMethod]
        public void TypeMap_RemoveTypeGeneric_RemovesType()
        {
            var typeMap = new TypeMap();
            typeMap.AddType<int>();
            ulong hash = typeMap.GetTypeHash<int>();

            typeMap.RemoveType<int>();

            Assert.ThrowsException<TypeMapException>(() => typeMap.GetTypeByHash(hash));
        }

    }
}
