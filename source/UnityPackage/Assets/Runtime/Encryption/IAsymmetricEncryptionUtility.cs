namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Asymmetric Encryption Utility. 
    /// Provides utility methods for assymetric encryption using Private/Public key pair.
    /// </summary>
    interface IAsymmetricEncryptionUtility
    {
        /// <summary>
        /// Asymmetric Public Key
        /// </summary>
        string PublicKey { get; }

        /// <summary>
        /// Encrypt given bytes with a public key
        /// </summary>
        /// <param name="bytes">Bytes to encrypt</param>
        /// <returns>Encrypted bytes</returns>
        byte[] Encrypt(byte[] bytes);

        /// <summary>
        /// Decrypts given bytes with a private key
        /// </summary>
        /// <param name="bytes">Bytes to decrypt</param>
        /// <returns>Decrypted bytes</returns>
        byte[] Decrypt(byte[] bytes);
    }
}