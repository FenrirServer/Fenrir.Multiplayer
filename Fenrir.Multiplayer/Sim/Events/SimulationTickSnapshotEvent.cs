﻿using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim.Dto;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Sim.Events
{
    public class SimulationTickSnapshotEvent : IEvent, IByteStreamSerializable
    {
        public LinkedList<SimulationTickSnapshot> TickSnapshots = new LinkedList<SimulationTickSnapshot>();

        public void Deserialize(IByteStreamReader reader)
        {
            // Read number of ticks
            byte numTicks = reader.ReadByte();

            for(int numTick=0; numTick<numTicks; numTick++)
            {
                var tickSnapshot = new SimulationTickSnapshot();
                tickSnapshot.Deserialize(reader);

                TickSnapshots.AddLast(tickSnapshot);
            }
        }

        public void Serialize(IByteStreamWriter writer)
        {
            // Write number of ticks
            if(TickSnapshots == null)
            {
                writer.Write((byte)0);
                return;
            }
            // TODO Check length of the snapshot list, should never exceed more than 256. It is does it means we are totally out of sync and need to possibly disconnect.
            writer.Write((byte)TickSnapshots.Count);

            // Write each tick snapshot
            foreach(var tickSnapshot in TickSnapshots)
            {
                tickSnapshot.Serialize(writer);
            }
        }
    }
}
