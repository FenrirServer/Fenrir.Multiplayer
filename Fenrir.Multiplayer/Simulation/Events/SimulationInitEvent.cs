using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Data;

namespace Fenrir.Multiplayer.Simulation.Events
{
    public class SimulationInitEvent : IEvent, IByteStreamSerializable
    {
        public SimulationTickSnapshot SimulationSnapshot;

        public SimulationInitEvent()
        {
        }

        public void Deserialize(IByteStreamReader reader)
        {
            SimulationSnapshot = reader.Read<SimulationTickSnapshot>();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(SimulationSnapshot);
        }
    }
}
