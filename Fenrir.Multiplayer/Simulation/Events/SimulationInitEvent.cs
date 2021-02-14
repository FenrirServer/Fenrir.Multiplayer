using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Data;

namespace Fenrir.Multiplayer.Simulation.Events
{
    public class SimulationInitEvent : IEvent, IByteStreamSerializable
    {
        private readonly NetworkSimulation _simulation;

        public SimulationTickSnapshot SimulationSnapshot;

        public SimulationInitEvent(NetworkSimulation simulation)
        {
            _simulation = simulation;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            SimulationSnapshot = new SimulationTickSnapshot(_simulation);
            SimulationSnapshot.Deserialize(reader);
        }

        public void Serialize(IByteStreamWriter writer)
        {
            SimulationSnapshot.Serialize(writer);
        }
    }
}
