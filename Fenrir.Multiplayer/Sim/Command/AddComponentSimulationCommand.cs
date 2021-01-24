using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class AddComponentSimulationCommand : ISimulationCommand
    {
        public CommandType Type => CommandType.AddComponent;

        public DateTime Time { get; private set; }
                
        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public AddComponentSimulationCommand(DateTime time, ushort objectId, ulong componentTypeHash)
        {
            Time = time;
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
        }
    }
}
