using System;

namespace Fenrir.Multiplayer
{
    interface ISymmetricEncryptionUtility : IDisposable
    {
        byte[] SymmetricKey { get; }

        byte[] Encrypt(byte[] bytes, int startIndex, int length);

        byte[] Decrypt(byte[] bytes, int startIndex, int length);
    }
}
