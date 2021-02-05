using System;

namespace Fenrir.Multiplayer.Sim.Exceptions
{
    public class NotInTickException : SimulationException
    {
        public NotInTickException()
        {
        }

        public NotInTickException(string message)
            : base(message)
        {
        }

        public NotInTickException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
