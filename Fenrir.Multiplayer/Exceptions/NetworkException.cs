using System;

namespace Fenrir.Multiplayer.Exceptions
{
    public class NetworkException : FenrirException
    {
        public NetworkException()
        {
        }

        public NetworkException(string message)
            : base(message)
        {
        }

        public NetworkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
