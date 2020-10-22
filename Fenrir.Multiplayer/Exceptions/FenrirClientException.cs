using System;

namespace Fenrir.Multiplayer.Exceptions
{
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
