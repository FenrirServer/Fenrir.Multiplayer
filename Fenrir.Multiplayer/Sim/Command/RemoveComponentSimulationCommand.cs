using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class RemoveComponentSimulationCommand : ISimulationCommand
    {
        public CommandType Type => CommandType.RemoveComponent;

        public DateTime Time { get; private set; }

        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public RemoveComponentSimulationCommand(DateTime time, ushort objectId, ulong componentTypeHash)
        {
            Time = time;
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
        }
    }
}
