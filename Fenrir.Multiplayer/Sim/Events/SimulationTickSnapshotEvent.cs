using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim.Command;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Fenrir.Multiplayer.Sim.Events
{
    // TODO : Move these to the PlayerComponent or separate place !!

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

    public class SimulationTickSnapshot : IByteStreamSerializable
    {
        public DateTime Time;

        public Dictionary<CommandType, SimulationCommandListSnapshot> Snapshots = new Dictionary<CommandType, SimulationCommandListSnapshot>();

        public void Deserialize(IByteStreamReader reader)
        {
            // Read time
            long ticks = reader.ReadLong();
            Time = new DateTime(ticks);

            // Read number of snapshots
            int numSnapshots = reader.ReadByte();

            // Read snapshots
            for(int i=0; i<numSnapshots; i++)
            {
                var snapshot = new SimulationCommandListSnapshot();
                snapshot.ReadCommandSnapshot(Time, reader);

                Snapshots.Add(snapshot.CommandType, snapshot);
            }
        }

        public void Serialize(IByteStreamWriter writer)
        {
            // Write time TODO: !!!!!!!!!!! use actual server tick number, not long DateTime.Ticks. 
            // Since this can't happen in between server ticks client should be able to infer datetime from server tick # since
            // those should be synced either way
            writer.Write(Time.Ticks);

            // Write number of snapshots
            if(Snapshots == null)
            {
                writer.Write((byte)0);
                return;
            }
            // TODO: Check length of snapshot list. It could be more than 256 snapshots + commands per tick? Probably won't fit into MTU
            writer.Write((byte)Snapshots.Count);

            // Write command snapshots
            foreach(var snapshot in Snapshots.Values)
            {
                snapshot.WriteCommandSnapshot(writer);
            }
        }
    }

    public class SimulationCommandListSnapshot
    {
        public CommandType CommandType;

        public List<ISimulationCommand> Commands = new List<ISimulationCommand>();

        public void ReadCommandSnapshot(DateTime commandTime, IByteStreamReader reader)
        {
            // Read command type
            CommandType = (CommandType)reader.ReadByte();

            // Read commands
            switch (CommandType)
            {
                case CommandType.SpawnObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                    {
                        // Read number of objects
                        byte numObjects = reader.ReadByte();

                        // Allocate command array. TODO: Use list and re-use data structures
                        Commands = new List<ISimulationCommand>();

                        // Read object ids that were spawned
                        for(int i=0; i<numObjects; i++)
                        {
                            ushort objectId = reader.ReadUShort();
                            var cmd = new SpawnObjectSimulationCommand(commandTime, objectId);
                            Commands.Add(cmd);
                        }
                    }
                    break;
                case CommandType.DestroyObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                    {
                        // Read number of objects
                        byte numObjects = reader.ReadByte();

                        // Read object ids that were spawned
                        for (int numObject = 0; numObject < numObjects; numObject++)
                        {
                            ushort objectId = reader.ReadUShort();
                            var cmd = new DestroyObjectSimulationCommand(commandTime, objectId);
                            Commands.Add(cmd);
                        }
                    }
                    break;
                case CommandType.AddComponent: // [byte numObjects] [objectId [componentTypeHash], [componentTypeHash], ...]  [objectId [componentTypeHash], [componentTypeHash], ...] 
                    {
                        // Read number of objects
                        byte numObjects = reader.ReadByte();

                        // Read objectIds that had added components
                        for (int numObject = 0; numObject < numObjects; numObject++)
                        {
                            ushort objectId = reader.ReadUShort();

                            // Read number of component type hashes added
                            byte numComponents = reader.ReadByte();

                            // Read components
                            for(int numComponent=0; numComponent<numComponents; numComponent++)
                            {
                                ulong componentTypeHash = reader.ReadULong();
                                var cmd = new AddComponentSimulationCommand(commandTime, objectId, componentTypeHash);
                                Commands.Add(cmd);
                            }
                        }
                    }
                    break;
                case CommandType.RemoveComponent:
                    {
                        // Read number of objects
                        byte numObjects = reader.ReadByte();

                        // Read objectIds that had removed components
                        for (int numObject = 0; numObject < numObjects; numObject++)
                        {
                            ushort objectId = reader.ReadUShort();

                            // Read number of component type hashes removed
                            byte numComponents = reader.ReadByte();

                            // Read components
                            for (int numComponent = 0; numComponent < numComponents; numComponent++)
                            {
                                ulong componentTypeHash = reader.ReadULong();
                                var cmd = new RemoveComponentSimulationCommand(commandTime, objectId, componentTypeHash);
                                Commands.Add(cmd);
                            }
                        }
                    }
                    break;
                case CommandType.InvokeRpc:
                    // TODO
                    break;
                case CommandType.SetComponentState:
                    // TODO
                    break;
            }
        }

        public void WriteCommandSnapshot(IByteStreamWriter writer)
        {
            // Write command type
            writer.Write((byte)CommandType);

            // Write commands
            switch (CommandType)
            {
                case CommandType.SpawnObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                    {
                        // Write number of objects
                        // TODO: Check length of commands. Could it be more than 256 commands per single tick? Probably won't fit into MTU
                        writer.Write((byte)Commands.Count);

                        // Write tightly packed list of object ids that were spawned
                        foreach(var command in Commands)
                        {
                            SpawnObjectSimulationCommand cmd = (SpawnObjectSimulationCommand)command;
                            writer.Write((ushort)cmd.ObjectId);
                        }
                    }
                    break;
                case CommandType.DestroyObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                    {
                        // Write number of objects
                        // TODO: Check length of commands. Could it be more than 256 commands per single tick? Probably won't fit into MTU
                        writer.Write((byte)Commands.Count);

                        // Write tightly packed list of object ids that were spawned
                        foreach (var command in Commands)
                        {
                            DestroyObjectSimulationCommand cmd = (DestroyObjectSimulationCommand)command;
                            writer.Write((ushort)cmd.ObjectId);
                        }
                    }
                    break;
                case CommandType.AddComponent: // [byte numObjects] [objectId [componentTypeHash], [componentTypeHash], ...]  [objectId [componentTypeHash], [componentTypeHash], ...] 
                    {
                        // Sort commands by component type hash for more efficient packing
                        Commands.Sort((cmd1, cmd2) => ((AddComponentSimulationCommand)cmd1).ComponentTypeHash.CompareTo(((AddComponentSimulationCommand)cmd2).ComponentTypeHash));

                        // This code assumes that commands written into this snapshot, are sorted by object id
                        // This should be always the case since component additions usually happen on per object id type, but
                        // just in case we also sort those. TODO: use pre-sorted data structure?

                        // First pass - count unique number of object ids
                        ushort objectId = 0;
                        byte numObjects = 0;

                        foreach (var command in Commands)
                        {
                            AddComponentSimulationCommand cmd = (AddComponentSimulationCommand)command;
                            if (cmd.ObjectId != objectId)
                            {
                                numObjects++;
                                objectId = cmd.ObjectId;
                            }
                        }

                        // Write number of objectIds that added components during this tick TODO: Check length?
                        writer.Write((byte)numObjects);

                        // Iterate over all commands, for each unique object id, count how many components added to it (until next objectId is hit)
                        objectId = 0;
                        for (int numCommand = 0; numCommand < Commands.Count; numCommand++)
                        {
                            AddComponentSimulationCommand command = (AddComponentSimulationCommand)Commands[numCommand];
                            if (command.ObjectId != objectId) // next objectId is found
                            {
                                objectId = command.ObjectId;

                                // Write object id
                                writer.Write((ushort)objectId);

                                // Look ahead and count number of components added to this object
                                byte numComponentsAdded = 0;
                                for (int numCmd = numCommand; numCmd < Commands.Count; numCmd++)
                                {
                                    AddComponentSimulationCommand cmd = (AddComponentSimulationCommand)Commands[numCmd];
                                    if (cmd.ObjectId != objectId)
                                    {
                                        break; // Found next object id, stop counting components
                                    }
                                    numComponentsAdded++;
                                }

                                // Write number of components added to this object
                                writer.Write((byte)numComponentsAdded);
                            }

                            // Write component hash
                            writer.Write((ulong)command.ComponentTypeHash);
                        }
                    }
                    break;
                case CommandType.RemoveComponent:
                    {
                        // Sort commands by component type hash for more efficient packing
                        Commands.Sort((cmd1, cmd2) => ((RemoveComponentSimulationCommand)cmd1).ComponentTypeHash.CompareTo(((RemoveComponentSimulationCommand)cmd2).ComponentTypeHash));

                        // First pass - count unique number of object ids
                        ushort objectId = 0;
                        byte numObjects = 0;

                        foreach (var command in Commands)
                        {
                            RemoveComponentSimulationCommand cmd = (RemoveComponentSimulationCommand)command;
                            if (cmd.ObjectId != objectId)
                            {
                                numObjects++;
                                objectId = cmd.ObjectId;
                            }
                        }

                        // Write number of objectIds that removed components during this tick TODO: Check length?
                        writer.Write((byte)numObjects);

                        // Iterate over all commands, for each unique object id, count how many components removed from it (until next objectId is hit)
                        objectId = 0;
                        for (int numCommand = 0; numCommand < Commands.Count; numCommand++)
                        {
                            RemoveComponentSimulationCommand command = (RemoveComponentSimulationCommand)Commands[numCommand];
                            if (command.ObjectId != objectId) // next objectId is found
                            {
                                objectId = command.ObjectId;

                                // Write object id
                                writer.Write((ushort)objectId);

                                // Look ahead and count number of components removed from this object

                                byte numComponentsRemoved = 0;
                                for (int numCmd = numCommand; numCmd < Commands.Count; numCmd++)
                                {
                                    RemoveComponentSimulationCommand cmd = (RemoveComponentSimulationCommand)Commands[numCmd];
                                    if (cmd.ObjectId != objectId)
                                    {
                                        break; // Found next object id, stop counting components
                                    }
                                    numComponentsRemoved++;
                                }

                                // Write number of components removed from this object
                                writer.Write((byte)numComponentsRemoved);
                            }

                            // Write component hash
                            writer.Write((ulong)command.ComponentTypeHash);
                        }
                    }
                    break;
                case CommandType.InvokeRpc:
                    // TODO
                    break;
                case CommandType.SetComponentState:
                    {
                        // Sort commands by component type hash + property
                        // Commands.Sort((cmd1, cmd2) => ((RemoveComponentSimulationCommand)cmd1).ComponentTypeHash.CompareTo(((RemoveComponentSimulationCommand)cmd2).ComponentTypeHash));


                        // !!!!!!!!!!!!!!!!!!!!!!!!!

                        // Code to tighly pack component updates into an efficient structure
                        // To do so, code below makes 2 assumptions: 
                        // 1. Component hash is longer than object id
                        // 2. Commands in a single snapshot (per command type) are sorted by component hash. For example: 
                        //      Command: component hash 1, object id 1
                        //      Command: component hash 1, object id 2
                        //      Command: component hash 2, object id 1
                        //      Command: component hash 2, object id 3

                        // With these 2 assumptions met, it is much more efficient to pack by component hash instead of by object id.
                        // So instead of: 
                        // {"obj1": ["long_comp_1", "long_comp_2", "long_comp_3"], "obj2": ["long_comp_1", "long_comp_2", "long_comp_3"]}
                        // instead we do:
                        // {"long_comp_1": ["obj1, obj2"], "long_comp_2": ["obj1, obj2"], "long_comp_3": ["obj1, obj2"]}

                        // This approach wins when you have SIMILAR component changes to multiple objects, for example all objects are constantly moving.
                        // So same components are always being updated on multiple objects.
                        // This may or may not be a win for component adds/removals, since those are usually one-offs per object.

                        // !!!!!!!!!!!!!!!!!!!!!!


                        /* 
                         
                                                 
                        // This code assumes that commands written into this snapshot, are sorted by component hash.
                        // Since they all happened during the same tick.

                        // First pass - count unique components
                        byte numUniqueComponents = 0;
                        ulong componentTypeHash = 0;
                        foreach(var command in Commands)
                        {
                            AddComponentSimulationCommand cmd = (AddComponentSimulationCommand)command;
                            if (cmd.ComponentTypeHash != componentTypeHash)
                            {
                                numUniqueComponents++;
                                componentTypeHash = cmd.ComponentTypeHash;
                            }
                        }

                        // Write number of unique components added during this tick TODO: Check length?
                        writer.Write(numUniqueComponents);

                        // Iterate over all commands, for each unique component, count how many objects it was added to (until next component hash is hit)
                        componentTypeHash = 0;
                        for (int numCommand = 0; numCommand < Commands.Length; numCommand++)
                        {
                            AddComponentSimulationCommand command = (AddComponentSimulationCommand)Commands[numCommand];
                            if (command.ComponentTypeHash != componentTypeHash)
                            {
                                componentTypeHash = command.ComponentTypeHash;

                                // Write component hash, only if new component
                                writer.Write((ulong)componentTypeHash);

                                // Look ahead and count number of objects that this component was added to
                                byte numObjectsPerComponent = 0;
                                for(int numCmd = numCommand; numCmd < Commands.Length; numCmd++)
                                {
                                    AddComponentSimulationCommand cmd = (AddComponentSimulationCommand)Commands[numCmd];
                                    if(cmd.ComponentTypeHash != componentTypeHash)
                                    {
                                        break; // Found next component hash, stop counting objects
                                    }
                                    numObjectsPerComponent++;
                                }

                                // Write number of objects
                                writer.Write((byte)numObjectsPerComponent);
                            }

                            // Write object id
                            writer.Write((ushort) command.ObjectId);
                        }
                         
                         
                         
                         
                         */
                    }
                    break;
            }
        }
    }
}
