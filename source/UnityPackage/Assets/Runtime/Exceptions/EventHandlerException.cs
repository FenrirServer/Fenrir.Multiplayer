using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Thrown when Event Handler encounters an error
    /// </summary>
    public class EventHandlerException : NetworkException
    {
        public EventHandlerException()
        {
        }

        public EventHandlerException(string message)
            : base(message)
        {
        }

        public EventHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
