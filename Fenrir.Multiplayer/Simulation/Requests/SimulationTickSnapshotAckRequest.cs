using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Simulation.Requests
{
    public class SimulationTickSnapshotAckRequest : IRequest, IByteStreamSerializable
    {
        public uint TickNumber;

        public SimulationTickSnapshotAckRequest() { }

        public SimulationTickSnapshotAckRequest(uint tickNumber)
        {
            TickNumber = tickNumber;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            TickNumber = reader.ReadUInt();
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(TickNumber);
        }
    }
}
