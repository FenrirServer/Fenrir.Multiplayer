using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim.Data;

namespace Fenrir.Multiplayer.Sim.Events
{
    public class SimulationInitEvent : IEvent, IByteStreamSerializable
    {
        public SimulationTickSnapshot SimulationSnapshot;

        public void Deserialize(IByteStreamReader reader)
        {
            SimulationSnapshot = new SimulationTickSnapshot();
            SimulationSnapshot.Deserialize(reader);
        }

        public void Serialize(IByteStreamWriter writer)
        {
            SimulationSnapshot.Serialize(writer);
        }
    }
}
