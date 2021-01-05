using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Type map stores deterministic type hashes used for dispatching
    /// and serialization/deserialization of messages
    /// </summary>
    interface ITypeHashMap
    {
        /// <summary>
        /// Adds type to the type map
        /// </summary>
        /// <typeparam name="T">Type to add</typeparam>
        void AddType<T>();

        /// <summary>
        /// Adds type to the type map
        /// </summary>
        /// <param name="type">Type to add</param>
        void AddType(Type type);

        /// <summary>
        /// Removes type
        /// </summary>
        /// <param name="type">Type to remove</param>
        void RemoveType(Type type);

        /// <summary>
        /// Removes type
        /// </summary>
        /// <typeparam name="T">Type to remove</typeparam>
        void RemoveType<T>();

        /// <summary>
        /// Calculates deterministic hash for a given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Hash for a given type</returns>
        ulong GetTypeHash<T>();

        /// <summary>
        /// Calculates deterministic hash for a given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Hash for a given type</returns>
        ulong GetTypeHash(Type type);

        /// <summary>
        /// Finds type for a given hash
        /// </summary>
        /// <param name="hash">Hash of the type</param>
        /// <returns>Type</returns>
        Type GetTypeByHash(ulong hash);

        /// <summary>
        /// Attempts to find type for a given hash
        /// </summary>
        /// <param name="hash">Hash of the type</param>
        /// <param name="type">Out parameter for a type, if found</param>
        /// <returns>True if type is found, otherwise false</returns>
        bool TryGetTypeByHash(ulong hash, out Type type);

        /// <summary>
        /// Checks if has map contains a given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True if type map contains a given type, otherwise false</returns>
        bool HasTypeHash(Type type);

        /// <summary>
        /// Checks if has map contains a given type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>True if type map contains a given type, otherwise false</returns>
        bool HasTypeHash<T>();
    }
}