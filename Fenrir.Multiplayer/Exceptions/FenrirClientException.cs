using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Base Fenrir Client exception
    /// Thrown on the client
    /// </summary>
    public class FenrirClientException : FenrirException
    {
        public FenrirClientException()
        {
        }

        public FenrirClientException(string message)
            : base(message)
        {
        }

        public FenrirClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
