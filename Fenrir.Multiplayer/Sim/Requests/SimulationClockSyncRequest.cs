using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;

namespace Fenrir.Multiplayer.Sim.Requests
{
    /// <summary>
    /// Request sent to synchronize simulation clock
    /// </summary>
    public class SimulationClockSyncRequest : IRequest<SimulationClockSyncResponse>, IByteStreamSerializable
    {
        public DateTime ClientTime;

        public SimulationClockSyncRequest()
        {
        }

        public SimulationClockSyncRequest(DateTime clientTime)
        {
            ClientTime = clientTime;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            ClientTime = new DateTime(reader.ReadLong());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(ClientTime.Ticks);
        }
    }

    /// <summary>
    /// Response to clock synchronization request
    /// </summary>
    public class SimulationClockSyncResponse : IResponse, IByteStreamSerializable
    {
        public DateTime ServerTime;

        public SimulationClockSyncResponse()
        {
        }

        public SimulationClockSyncResponse(DateTime serverTime)
        {
            ServerTime = serverTime;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            ServerTime = new DateTime(reader.ReadLong());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(ServerTime.Ticks);
        }
    }
}
