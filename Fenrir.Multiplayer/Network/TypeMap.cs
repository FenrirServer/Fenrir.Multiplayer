using Fenrir.Multiplayer.Exceptions;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Type map - calculates and stores deterministic hash code for each System.Type
    /// </summary>
    public class TypeMap : ITypeMap
    {
        private Dictionary<ulong, Type> _hashToTypeDictionary = new Dictionary<ulong, Type>();
        private Dictionary<Type, ulong> _typeToHashDictionary = new Dictionary<Type, ulong>();

        private object _syncRoot = new object();

        /// <inheritdoc/>
        public void AddType<T>()
        {
            AddType(typeof(T));
        }

        /// <inheritdoc/>
        public void AddType(Type type)
        {
            lock(_syncRoot)
            {
                if (_typeToHashDictionary.ContainsKey(type))
                {
                    return;
                }
            }

            ulong hash = CalculateTypeHash(type);

            lock(_syncRoot)
            {
                _hashToTypeDictionary[hash] = type;
                _typeToHashDictionary[type] = hash;
            }
        }

        /// <inheritdoc/>
        public void RemoveType(Type type)
        {
            lock (_syncRoot)
            {
                if (!_typeToHashDictionary.ContainsKey(type))
                {
                    return;
                }

                ulong hash = _typeToHashDictionary[type];

                _typeToHashDictionary.Remove(type);
                _hashToTypeDictionary.Remove(hash);
            }
        }

        /// <inheritdoc/>
        public ulong GetTypeHash<T>()
        {
            return GetTypeHash(typeof(T));
        }

        /// <inheritdoc/>
        public ulong GetTypeHash(Type type)
        {
            ulong hash;

            // Try to find cached hash
            lock(_syncRoot)
            {
                if(_typeToHashDictionary.TryGetValue(type, out hash))
                {
                    return hash;
                }
            }

            // No hash found, calculate
            hash = CalculateTypeHash(type);

            lock (_syncRoot)
            {
                _hashToTypeDictionary[hash] = type;
                _typeToHashDictionary[type] = hash;
            }

            return hash;
        }

        /// <inheritdoc/>
        public Type GetTypeByHash(ulong hash)
        {
            lock(_syncRoot)
            {
                if (!_hashToTypeDictionary.ContainsKey(hash))
                {
                    throw new TypeMapException($"Failed ot get type for the hash {hash}, type not found");
                }

                return _hashToTypeDictionary[hash];
            }
        }

        /// <summary>
        /// Calculates deterministic type name hash using fnv-1
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Deterministic type hash</returns>
        private ulong CalculateTypeHash(Type type)
        {
            // Calculates fnv-1 64 bit hash of the type name

            ulong hash = 14695981039346656037UL; // Offset
            string typeName = type.FullName;

            for (var i = 0; i < typeName.Length; i++)
            {
                hash = hash ^ typeName[i];
                hash *= 1099511628211UL; // Prime
            }

            return hash;
        }

    }
}
