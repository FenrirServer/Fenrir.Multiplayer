using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using System;

namespace Fenrir.Multiplayer.Sim.Events
{
    /// <summary>
    /// Event sent to synchronize simulation clock
    /// </summary>
    class SimulationClockSyncAckEvent : IEvent, IByteStreamSerializable
    {
        public DateTime TimeSentRequest;
        public DateTime TimeReceivedRequest;
        public DateTime TimeSentResponse;

        public SimulationClockSyncAckEvent()
        {
        }

        public SimulationClockSyncAckEvent(DateTime timeSentRequest, DateTime timeReceivedRequest)
        {
            TimeSentRequest = timeSentRequest;
            TimeReceivedRequest = timeReceivedRequest;
            TimeSentResponse = DateTime.UtcNow;
        }

        public void Deserialize(IByteStreamReader reader)
        {
            TimeSentRequest = new DateTime(reader.ReadLong());
            TimeReceivedRequest = new DateTime(reader.ReadLong());
            TimeSentResponse = new DateTime(reader.ReadLong());
        }

        public void Serialize(IByteStreamWriter writer)
        {
            // Probably not much difference between this and request received time, couple ticks.. 
            TimeSentResponse = DateTime.UtcNow;

            writer.Write(TimeSentRequest.Ticks);
            writer.Write(TimeReceivedRequest.Ticks);
            writer.Write(TimeSentResponse.Ticks);
        }
    }
}
