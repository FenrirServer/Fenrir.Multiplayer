using Fenrir.Multiplayer.Exceptions;
using System;

namespace Fenrir.Multiplayer.Sim.Exceptions
{
    class SimulationException : FenrirException
    {
        public SimulationException()
        {
        }

        public SimulationException(string message)
            : base(message)
        {
        }

        public SimulationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
