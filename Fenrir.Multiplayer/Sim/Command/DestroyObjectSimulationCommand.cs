using System;

namespace Fenrir.Multiplayer.Sim.Command
{
    class DestroyObjectSimulationCommand : ISimulationCommand
    {
        public CommandType Type => CommandType.DestroyObject;

        public DateTime Time { get; private set; }

        public ushort ObjectId { get; private set; }

        public DestroyObjectSimulationCommand(DateTime time, ushort objectId)
        {
            Time = time;
            ObjectId = objectId;
        }
    }
}
