using System;

namespace Fenrir.Multiplayer.Simulation.Command
{
    class RemoveComponentSimulationCommand : IComponentSimulationCommand
    {
        public CommandType Type => CommandType.RemoveComponent;

        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public RemoveComponentSimulationCommand(ushort objectId, ulong componentTypeHash)
        {
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
        }
    }
}
