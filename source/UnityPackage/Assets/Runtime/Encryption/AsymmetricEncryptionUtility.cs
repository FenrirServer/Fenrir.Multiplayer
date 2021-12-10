using System;
using System.Security.Cryptography;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Asymmetric Encryption Utility. 
    /// Provides utility methods for assymetric encryption using Private/Public key pair.
    /// </summary>
    class AsymmetricEncryptionUtility : IAsymmetricEncryptionUtility, IDisposable
    {
        /// <summary>
        /// RSA Crypto Service Provider
        /// </summary>
        private RSACryptoServiceProvider _cryptoServiceProvider;

        /// <summary>
        /// Stores public key
        /// </summary>
        private string _publicKey;

        /// <summary>
        /// Public Key
        /// </summary>
        public string PublicKey => _publicKey;

        /// <summary>
        /// Default key size
        /// </summary>
        private const int DefaultKeySizeBytes = 2048;

        /// <summary>
        /// Creates new Assymetric Encryption Utility
        /// </summary>
        public AsymmetricEncryptionUtility()
        {
            // Create new RSACryptoServiceProvider
            _cryptoServiceProvider = new RSACryptoServiceProvider(DefaultKeySizeBytes);

            // Create new public key
            _publicKey = _cryptoServiceProvider.ToXmlString(false);
        }

        /// <summary>
        /// Creates new Asymmetric Encryption Utility using known Public Key
        /// </summary>
        /// <param name="publicKey">Public Key</param>
        public AsymmetricEncryptionUtility(string publicKey)
        {
            // Create new RSACryptoServiceProvider
            _cryptoServiceProvider = new RSACryptoServiceProvider();

            // Set public key
            _cryptoServiceProvider.FromXmlString(publicKey);

            // Set public key
            _publicKey = publicKey;
        }

        /// <summary>
        /// Encrypt given bytes with a public key
        /// </summary>
        /// <param name="bytes">Bytes to encrypt</param>
        /// <returns>Encrypted bytes</returns>
        /// <exception cref="CryptographicException"/>
        /// <exception cref="ArgumentNullException"/>
        public byte[] Encrypt(byte[] bytes)
        {
            if(bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return _cryptoServiceProvider.Encrypt(bytes, false);
        }

        /// <summary>
        /// Decrypts given bytes with a private key
        /// </summary>
        /// <param name="bytes">Bytes to decrypt</param>
        /// <returns>Decrypted bytes</returns>
        /// <exception cref="CryptographicException"/>
        /// <exception cref="ArgumentNullException"/>
        public byte[] Decrypt(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            return _cryptoServiceProvider.Decrypt(bytes, false);
        }

        /// <summary>
        /// Disposes Assymetric Encryption Utility
        /// </summary>
        public void Dispose()
        {
            _cryptoServiceProvider?.Clear();
            _cryptoServiceProvider = null;
            _publicKey = null;
        }
    }
}
