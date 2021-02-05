using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class AddComponentSimulationCommand : IComponentSimulationCommand
    {
        public CommandType Type => CommandType.AddComponent;

        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public AddComponentSimulationCommand(ushort objectId, ulong componentTypeHash)
        {
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
        }
    }
}
