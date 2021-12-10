using System;

namespace Fenrir.Multiplayer
{
    class SymmetricEncryptionUtility : ISymmetricEncryptionUtility, IDisposable
    {
        // TODO totally need to re-write

        public byte[] SymmetricKey => _symmetricEncryptionKey;

        byte[] _symmetricEncryptionKey;
        
        public SymmetricEncryptionUtility()
        {
            _symmetricEncryptionKey = new byte[128];
            for(int i =0; i<_symmetricEncryptionKey.Length; i++)
            {
                _symmetricEncryptionKey[i] = (byte)i; // very secure
            }
        }

        public SymmetricEncryptionUtility(byte[] symmetricEncryptionKey)
        {
            _symmetricEncryptionKey = symmetricEncryptionKey;
        }

        public byte[] Encrypt(byte[] bytes, int startIndex, int length)
        {
            for(int i = 0; i < length; i++)
            {
                bytes[startIndex + i] = (byte)(bytes[startIndex + i] ^ _symmetricEncryptionKey[i % _symmetricEncryptionKey.Length]);
            }

            return bytes;
        }

        public byte[] Decrypt(byte[] bytes, int startIndex, int length)
        {
            for (int i = 0; i < length; i++)
            {
                bytes[startIndex + i] = (byte)(bytes[startIndex + i] ^ _symmetricEncryptionKey[i % _symmetricEncryptionKey.Length]);
            }

            return bytes;
        }

        public void Dispose()
        {
            _symmetricEncryptionKey = null;
        }
    }
}
