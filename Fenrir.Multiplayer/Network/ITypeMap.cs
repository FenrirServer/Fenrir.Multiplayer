using System;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Type map stores deterministic type hashes used for dispatching
    /// and serialization/deserialization of messages
    /// </summary>
    interface ITypeMap
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
    }
}