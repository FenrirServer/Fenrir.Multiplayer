using System;

namespace Fenrir.Multiplayer.Sim.State
{
    struct SimulationComponentSnapshot
    {
        public Type ComponentType { get; private set; }

        public ulong ComponentTypeHash { get; private set; }


    }
}
