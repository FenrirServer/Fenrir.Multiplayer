﻿using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Data;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Events
{
    class SimulationTickSnapshotEvent : IEvent, IByteStreamSerializable
    {
        public LinkedList<SimulationTickSnapshot> TickSnapshots = new LinkedList<SimulationTickSnapshot>();

        public SimulationTickSnapshotEvent()
        {
        }

        public void Deserialize(IByteStreamReader reader)
        {
            // Read number of ticks
            byte numTicks = reader.ReadByte();

            for(int numTick=0; numTick<numTicks; numTick++)
            {
                var tickSnapshot = reader.Read<SimulationTickSnapshot>();
                TickSnapshots.AddLast(tickSnapshot);
            }
        }

        public void Serialize(IByteStreamWriter writer)
        {
            // Write number of ticks
            if (TickSnapshots == null)
            {
                writer.Write((byte)0);
                return;
            }

            // TODO Check length of the snapshot list, should never exceed more than 256. It is does it means we are totally out of sync and need to possibly disconnect.
            writer.Write((byte)TickSnapshots.Count);

            // Write each tick snapshot
            foreach(var tickSnapshot in TickSnapshots)
            {
                writer.Write(tickSnapshot);
            }
        }
    }
}