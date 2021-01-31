using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;

namespace Fenrir.Multiplayer.Sim.Requests
{
    /// <summary>
    /// Request sent to synchronize simulation clock
    /// </summary>
    public class SimulationClockSyncRequest : IRequest, IByteStreamSerializable
    {
        public DateTime RequestSentTime;

        public SimulationClockSyncRequest()
        {
        }

        public SimulationClockSyncRequest(DateTime clientTime)
        {
            RequestSentTime = clientTime;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            RequestSentTime = new DateTime(reader.ReadLong());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(RequestSentTime.Ticks);
        }
    }

}
