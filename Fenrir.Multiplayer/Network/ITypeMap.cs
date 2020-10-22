using System;

namespace Fenrir.Multiplayer.Network
{
    public interface ITypeMap
    {
        void AddType<T>();

        void AddType(Type type);

        void RemoveType(Type type);

        ulong GetTypeHash<T>();
        ulong GetTypeHash(Type type);

        Type GetTypeByHashInternal(ulong hash);
    }
}