using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class SpawnObjectSimulationCommand : ISimulationCommand
    {
        public CommandType Type => CommandType.SpawnObject;

        public DateTime Time { get; private set; }

        public ushort ObjectId { get; private set; }


        public SpawnObjectSimulationCommand(DateTime time, ushort objectId)
        {
            Time = time;
            ObjectId = objectId;
        }
    }
}
