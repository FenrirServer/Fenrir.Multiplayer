using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Data;
using System;

namespace Fenrir.Multiplayer.Simulation.Events
{
    public class SimulationInitEvent : IEvent, IByteStreamSerializable
    {
        public int SimulationTickRate;

        public SimulationTickSnapshot InitialSnapshot;


        public SimulationInitEvent()
        {
        }

        public SimulationInitEvent(int simulationTickRate, DateTime initialTickTime, SimulationTickSnapshot initialSnapshot)
        {
            SimulationTickRate = simulationTickRate;
            InitialSnapshot = initialSnapshot;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            SimulationTickRate = reader.ReadInt();
            InitialSnapshot = reader.Read<SimulationTickSnapshot>();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(SimulationTickRate);
            writer.Write(InitialSnapshot);
        }
    }
}
