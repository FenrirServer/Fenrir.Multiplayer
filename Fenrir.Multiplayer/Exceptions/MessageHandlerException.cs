using System;

namespace Fenrir.Multiplayer.Exceptions
{
    /// <summary>
    /// Thrown when raw message handler encounters an error
    /// </summary>
    public class MessageHandlerException : NetworkException
    {
        public MessageHandlerException()
        {
        }

        public MessageHandlerException(string message)
            : base(message)
        {
        }

        public MessageHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
