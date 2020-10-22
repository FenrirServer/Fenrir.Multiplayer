using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Exceptions
{
    public class EventListenerException : NetworkException
    {
        public EventListenerException()
        {
        }

        public EventListenerException(string message)
            : base(message)
        {
        }

        public EventListenerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
