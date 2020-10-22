using System;

namespace Fenrir.Multiplayer.Exceptions
{
    public class SerializationException : FenrirException
    {
        public SerializationException()
        {
        }

        public SerializationException(string message)
            : base(message)
        {
        }

        public SerializationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
