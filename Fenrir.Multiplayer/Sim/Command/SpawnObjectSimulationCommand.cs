using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class SpawnObjectSimulationCommand : IObjectSimulationCommand
    {
        public CommandType Type => CommandType.SpawnObject;

        public ushort ObjectId { get; private set; }


        public SpawnObjectSimulationCommand(ushort objectId)
        {
            ObjectId = objectId;
        }
    }
}
