using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Sim.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Sim.Dto
{
    public class SimulationTickSnapshot : IByteStreamSerializable
    {
        public DateTime TickTime;

        public List<ISimulationCommand> Commands = new List<ISimulationCommand>();

        // TODO List of component state changes

        public void Deserialize(IByteStreamReader reader)
        {
            // Read time
            long ticks = reader.ReadLong();
            TickTime = new DateTime(ticks);

            // Read number of commands
            byte numCommands = reader.ReadByte();

            Commands = new List<ISimulationCommand>(numCommands);

            // Read commands
            byte numCommand = 0;
            while (numCommand < numCommands)
            {
                // Read command type
                CommandType commandType = (CommandType)reader.ReadByte();

                // Read commands
                switch (commandType)
                {
                    case CommandType.SpawnObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                        {
                            // Read number of objects packed into this compressed chunk of commands
                            byte numObjects = reader.ReadByte();

                            // Read object ids that were spawned
                            for (int i = 0; i < numObjects; i++)
                            {
                                ushort objectId = reader.ReadUShort();
                                var cmd = new SpawnObjectSimulationCommand(objectId);
                                Commands.Add(cmd);
                                numCommand++;
                            }
                        }
                        break;
                    case CommandType.DestroyObject: // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...
                        {
                            // Read number of objects
                            byte numObjects = reader.ReadByte();

                            // Read object ids that were spawned packed into this compressed chunk of commands
                            for (int numObject = 0; numObject < numObjects; numObject++)
                            {
                                ushort objectId = reader.ReadUShort();
                                var cmd = new DestroyObjectSimulationCommand(objectId);
                                Commands.Add(cmd);
                                numCommand++;
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
                                for (int numComponent = 0; numComponent < numComponents; numComponent++)
                                {
                                    ulong componentTypeHash = reader.ReadULong();
                                    var cmd = new AddComponentSimulationCommand(objectId, componentTypeHash);
                                    Commands.Add(cmd);
                                    numCommand++;
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
                                    var cmd = new RemoveComponentSimulationCommand(objectId, componentTypeHash);
                                    Commands.Add(cmd);
                                    numCommand++;
                                }
                            }
                        }
                        break;
                    case CommandType.InvokeRpc:
                        // TODO
                        throw new NotImplementedException();
                }
            }
        }

        public void Serialize(IByteStreamWriter writer)
        {
            writer.Write(TickTime.Ticks);

            // Write commands
            if (Commands != null)
            {
                WriteCommands(writer);
            }

            // Write states
            //if (States != null && States.Count > 0)
            //{
            //    WriteStates(writer);
            //}
        }

        private void WriteCommands(IByteStreamWriter writer)
        {
            // Consecutive commands of the same type, are packed:
            // [byte numCommands] [ [CommandType] [command] [command] ] [ [CommandType] [command] [command] ... ] 

            // Write total number of commands
            writer.Write((byte)Commands.Count);

            // Write commands
            int commandIndex = 0;
            while (commandIndex < Commands.Count)
            {
                ISimulationCommand command = Commands[commandIndex];

                // Write command type
                writer.Write((byte)command.Type);

                // Write command block
                switch (command.Type)
                {
                    case CommandType.SpawnObject:
                    case CommandType.DestroyObject:
                        WriteObjectCommandBlock(writer, ref commandIndex);
                        break;
                    case CommandType.AddComponent:
                    case CommandType.RemoveComponent:
                        WriteComponentCommandBlock(writer, ref commandIndex);
                        break;
                    case CommandType.InvokeRpc:
                        WriteRpcCommandBlock(writer, ref commandIndex);
                        break;
                }
            }
        }

        private void WriteObjectCommandBlock(IByteStreamWriter writer, ref int commandIndex)
        {
            // Spawn / Destroy object commands. Consecutive commands are tightly packed:
            // [byte numObjectIds] [ushort objectId] [ushort objectId] [ushort objectId]...

            CommandType commandType = Commands[commandIndex].Type;

            // Look ahead and count how many back-to-back spawn/destroy object commands we have AKA block size
            byte numObjects = 1;

            while (commandIndex + numObjects < Commands.Count
                && Commands[commandIndex + numObjects].Type == commandType)
            {
                numObjects++;
            }

            // Write how many objects we have AKA block size
            writer.Write(numObjects);

            // Iterate over spawn/destroy object commands and write object ids
            int startIndex = commandIndex;
            while (commandIndex < startIndex + numObjects)
            {
                IObjectSimulationCommand cmd = (IObjectSimulationCommand)Commands[commandIndex];
                writer.Write(cmd.ObjectId);
                commandIndex++;
            }
        }

        private void WriteComponentCommandBlock(IByteStreamWriter writer, ref int commandIndex)
        {
            // Add / Remove component commands. Consecutive commands are tightly packed:
            // [byte numObjects] [objectId [componentTypeHash], [componentTypeHash], ...]  [objectId [componentTypeHash], [componentTypeHash], ...] 

            CommandType commandType = Commands[commandIndex].Type;

            // Look ahead and count how many back-to-back component object commands we have with unique objectIds, AKA block size
            byte numObjects = 1;

            while (commandIndex + numObjects < Commands.Count
                && Commands[commandIndex + numObjects].Type == commandType)
            {
                numObjects++;
            }

            // Write how many objects we have AKA block size
            writer.Write(numObjects);

            // Iterate over component commands and write object ids
            int startIndex = commandIndex;
            while (commandIndex < startIndex + numObjects)
            {
                IComponentSimulationCommand cmd = (IComponentSimulationCommand)Commands[commandIndex];
             
                writer.Write(cmd.ObjectId);

                // Look ahead and count how many back-to-back component commands we have with unique component type hash, AKA block size
                byte numComponents = 1;

                while (commandIndex + numComponents < Commands.Count
                    && Commands[commandIndex + numComponents].Type == commandType)
                {
                    numComponents++;
                }

                // Write number of components
                writer.Write(numComponents);

                // Write component hashes
                for(int numComponent = 0; numComponent < numComponents; numComponent++)
                {
                    IComponentSimulationCommand command = (IComponentSimulationCommand)Commands[commandIndex];
                    writer.Write(command.ComponentTypeHash);
                    commandIndex++;
                }
            }
        }

        private void WriteRpcCommandBlock(IByteStreamWriter writer, ref int commandIndex)
        {
        }

        private void WriteStates(IByteStreamWriter writer)
        {
        }
    }
}
