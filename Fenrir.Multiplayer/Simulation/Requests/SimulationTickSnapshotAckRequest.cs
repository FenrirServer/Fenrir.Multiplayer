using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;

namespace Fenrir.Multiplayer.Simulation.Requests
{
    public class SimulationTickSnapshotAckRequest : IRequest, IByteStreamSerializable
    {
        public DateTime TickTime;

        public SimulationTickSnapshotAckRequest() { }

        public SimulationTickSnapshotAckRequest(DateTime tickTime)
        {
            TickTime = tickTime;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            long timeTicks = reader.ReadLong();
            TickTime = new DateTime(timeTicks);
        }

        public void Serialize(IByteStreamWriter writer)
        {
            long ticks = TickTime.Ticks;
            writer.Write(ticks);
        }
    }
}
