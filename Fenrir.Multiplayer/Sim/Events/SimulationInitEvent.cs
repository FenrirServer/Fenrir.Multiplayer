using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim.Data;

namespace Fenrir.Multiplayer.Sim.Events
{
    public class SimulationInitEvent : IEvent, IByteStreamSerializable
    {
        private readonly Simulation _simulation;

        public SimulationTickSnapshot SimulationSnapshot;

        public SimulationInitEvent(Simulation simulation)
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
