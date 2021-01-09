using System;

namespace Fenrir.Multiplayer.Sim.State.Command
{
    class CreateObjectStateCommand : ISimulationStateCommand
    {
        public ushort ObjectId { get; set; }

        public void Apply(Simulation sim)
        {
            sim.CreateObject();
        }

        public void Rollback(Simulation sim)
        {
        }
    }
}
