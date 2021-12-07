using System;

namespace Fenrir.Multiplayer
{
    /// <summary>
    /// Thrown when Event Handler encounters an error
    /// </summary>
    public class EventHandlerException : NetworkException
    {
        /// <summary>
        /// Creates Event Handler Exception
        /// </summary>
        public EventHandlerException()
        {
        }

        /// <summary>
        /// Creates Event Handler Exception using a string message
        /// </summary>
        /// <param name="message">Exception message</param>
        public EventHandlerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates Event Handler Exception using a string message and an inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public EventHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
