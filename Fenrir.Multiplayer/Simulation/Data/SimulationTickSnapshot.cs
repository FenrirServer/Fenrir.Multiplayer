using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Simulation.Command;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Fenrir.Multiplayer.Simulation.Data
{
    public class SimulationTickSnapshot : IByteStreamSerializable
    {
        private readonly NetworkSimulation _simulation;

        public DateTime TickTime;

        public List<ISimulationCommand> Commands = new List<ISimulationCommand>();

        // TODO List of component state changes

        public SimulationTickSnapshot(NetworkSimulation simulation)
        {
            _simulation = simulation;
        }

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

                    case CommandType.ClientRpc:
                    case CommandType.ServerRpc:
                        {
                            // Read number of component hashes
                            byte numComponents = reader.ReadByte();

                            // Read component hashes
                            for (int numComponent = 0; numComponent < numComponents; numComponent++)
                            {
                                ulong componentTypeHash = reader.ReadULong();

                                // Read number of method hashes of RPCs invoked for this component
                                byte numMethods = reader.ReadByte();

                                // Read method hashes
                                for (int numMethod = 0; numMethod < numMethods; numMethod++)
                                {
                                    ulong methodHash = reader.ReadULong();

                                    // Read number of objects this method was invoked on
                                    byte numObjects = reader.ReadByte();

                                    // Read objects and parameters
                                    for(int numObject = 0; numObject < numObjects; numObject++)
                                    {
                                        // Read object id
                                        ushort objectId = reader.ReadUShort();

                                        // Read RPC parameter types
                                        if (!_simulation.TryGetComponentTypeByHash(componentTypeHash, out Type componentType))
                                        {
                                            throw new SerializationException("Unknown component type hash, component not registered with Simulation: " + componentTypeHash);
                                        }

                                        var componentWrapper = _simulation.GetComponentWrapper(componentType);

                                        ComponentTypeWrapper.RpcParameterInfo[] parameterInfos;

                                        if (commandType == CommandType.ServerRpc)
                                        {
                                            // TODO: Here and below, change to TryGet... to normalize exception type
                                            parameterInfos = componentWrapper.GetServerRpcMethodInfo(methodHash).Parameters;
                                        }
                                        else
                                        {
                                            parameterInfos = componentWrapper.GetClientRpcMethodInfo(methodHash).Parameters;
                                        }

                                        // TODO: Remove allocation? Use object pool?
                                        object[] parameters = new object[parameterInfos.Length];

                                        // Read RPC parameter values
                                        for (int numParam = 0; numParam < parameterInfos.Length; numParam++)
                                        {
                                            Type parameterType = parameterInfos[numParam].ParameterType;

                                            // Read parameter value
                                            parameters[numParam] = reader.Read(parameterType);
                                        }

                                        IRpcSimulationCommand cmd;

                                        if (commandType == CommandType.ServerRpc)
                                        {
                                            cmd = new ServerRpcSimulationCommand(objectId, componentTypeHash, methodHash, parameters);
                                        }
                                        else
                                        {
                                            cmd = new ClientRpcSimulationCommand(objectId, componentTypeHash, methodHash, parameters);
                                        }

                                        Commands.Add(cmd);
                                        numCommand++;
                                    }
                                }
                            }
                        }
                        break;
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
            // Consecutive commands of the same type, are packed into blocks.
            // Each block contains compressed chain  commands of the same type.
            // [byte numCommands] [ObjectCommandBlock] [ComponentCommandBlock] [ObjectCommandBlock] [RpcCommandBlock]

            // Write total number of commands
            writer.Write((byte)Commands.Count);

            // Write command blocks
            int commandIndex = 0;
            while (commandIndex < Commands.Count)
            {
                // Write command block
                CommandType commandType = Commands[commandIndex].Type;

                // Write command block type
                writer.Write((byte)commandType);

                // Write command block based on the command type
                switch (commandType)
                {
                    case CommandType.SpawnObject:
                    case CommandType.DestroyObject:
                        WriteObjectCommandBlock(writer, ref commandIndex);
                        break;
                    case CommandType.AddComponent:
                    case CommandType.RemoveComponent:
                        WriteComponentCommandBlock(writer, ref commandIndex);
                        break;
                    case CommandType.ServerRpc:
                    case CommandType.ClientRpc:
                        WriteRpcCommandBlock(writer, ref commandIndex);
                        break;
                }
            }
        }


        private void WriteObjectCommandBlock(IByteStreamWriter writer, ref int firstCommandIndex)
        {
            // Packs block of commands of the same type (Spawn Object or Destroy Object)

            // From: [SpawnObjectSimulationCommand(objectId), SpawnObjectSimulationCommand(objectId), SpawnObjectSimulationCommand(objectId), ]
            // To: [numObjects, objectId, objectId, objectId]

            // Format:
            // byte numSameTypeCommands (block size, or number of objects in the block)
            //      ushort objectId
            //      ushort objectId

            CommandType commandType = Commands[firstCommandIndex].Type;

            // Look ahead and count how many command of the same type we have, aka block size
            byte numSameTypeCommands = 1;

            while (firstCommandIndex + numSameTypeCommands < Commands.Count
                && Commands[firstCommandIndex + numSameTypeCommands].Type == commandType)
            {
                numSameTypeCommands++;
            }

            // Write how many commands of the same type / objects we have AKA block size
            writer.Write(numSameTypeCommands);

            // Iterate over spawn/destroy object commands and write object ids
            int startIndex = firstCommandIndex;
            while (firstCommandIndex < startIndex + numSameTypeCommands)
            {
                IObjectSimulationCommand blockCommand = (IObjectSimulationCommand)Commands[firstCommandIndex];
                writer.Write(blockCommand.ObjectId);
                firstCommandIndex++;
            }
        }

        private void WriteComponentCommandBlock(IByteStreamWriter writer, ref int commandIndex)
        {
            // Packs block of commands of the same type (Add Component or Remove Component)

            // Format:
            // byte numSameTypeCommands (block size, or number of objectsIds in the block)
            //      ushort objectId
            //      byte numSameObjectIdCommands (sub-block size, or number component hashes in the sub-block)
            //          ulong componentTypeHash
            //          ulong componentTypeHash

            //      ushort objectId
            //      byte numSameObjectIdCommands
            //          ulong componentTypeHash
            //          ulong componentTypeHash


            // We prefer to write objectId -> list of components instead of component -> list of objects,
            // based on the assumption that same component is rarely added to "all objects" in a single tick.
            // More often, we will spawn an object and add a bunch of components to it in the same tick.

            CommandType commandType = Commands[commandIndex].Type;

            // Look ahead and count how many back-to-back component object commands we have with the same objectId, AKA object block size
            byte numSameTypeCommands = 1;

            while (commandIndex + numSameTypeCommands < Commands.Count
                && Commands[commandIndex + numSameTypeCommands].Type == commandType)
            {
                numSameTypeCommands++;
            }

            // Write how many commands of the same type we have AKA block size
            writer.Write(numSameTypeCommands);

            // Start writing this block.
            // Iterate over commands and write objectId, then size of the sub-block (number of type hashes)
            int blockStartIndex = commandIndex;
            while (commandIndex < blockStartIndex + numSameTypeCommands)
            {
                IComponentSimulationCommand blockCommand = (IComponentSimulationCommand)Commands[commandIndex];
             
                writer.Write(blockCommand.ObjectId);

                // Look ahead and count how many back-to-back component commands we have with the same object id, AKA sub-block size
                byte numSameObjectIdsCommands = 1;

                while (commandIndex + numSameObjectIdsCommands < Commands.Count
                    && Commands[commandIndex + numSameObjectIdsCommands].Type == commandType
                    && ((IComponentSimulationCommand)Commands[commandIndex + numSameObjectIdsCommands]).ObjectId == blockCommand.ObjectId)
                {
                    numSameObjectIdsCommands++;
                }

                // Write number of commands that have the same object id
                writer.Write(numSameObjectIdsCommands);

                // Write commands that has the same object, aka component hashes
                int subBlockStartIndex = commandIndex;
                while(commandIndex <  subBlockStartIndex + numSameObjectIdsCommands)
                {
                    IComponentSimulationCommand subBlockCommand = (IComponentSimulationCommand)Commands[commandIndex];
                    writer.Write(subBlockCommand.ComponentTypeHash);
                    commandIndex++;
                }
            }
        }



        private void WriteRpcCommandBlock(IByteStreamWriter writer, ref int commandIndex)
        {
            // Invoke component RPC. Consecutive commands are tightly packed.
            // We are assuming that for the most part, RPC will be invoked on same components but multiple objects

            // Format:
            // byte numSameTypeCommands (block size, or number of component hashes in the block)
            //      ulong componentTypeHash
            //      byte numSameComponentTypeHashCommands (sub-block size, or number method hashes in the sub-block)
            //          ulong methodHash
            //          byte numObjectIds (sub-sub-block size, or number of objectIds in the sub-block)
            //              ushort objectId, (serialized values)
            //              ushort objectId, (serialized values)
            //          ulong methodHash
            //          byte numObjectIds (sub-sub-block size, or number of objectIds in the sub-block)
            //              ushort objectId, (serialized values)
            //              ushort objectId, (serialized values)
            //      ulong componentTypeHash
            //      byte numSameComponentTypeHashCommands (sub-block size, or number method hashes in the sub-block)
            //          ulong methodHash
            //          byte numObjectIds (sub-sub-block size, or number of objectIds in the sub-block)
            //              ushort objectId, (serialized values)
            //              ushort objectId, (serialized values)


            // We prefer to write component hash -> method hashs -> list of objectIds, (instead of objectId -> components hashes -> method hashes)
            // based on the assumption that usually, we will call the same RPC on multiple objects each tick (e.g Move RPC, or Shoot RPC)
            // instead of calling multiple RPCs on a single object / component in a row.

            CommandType blockCommandType = Commands[commandIndex].Type;

            // Look ahead and count how many back-to-back RPC object commands we have with unique componentIds, AKA block size
            byte numSameTypeCommands = 1;
            while (commandIndex + numSameTypeCommands < Commands.Count
                && Commands[commandIndex + numSameTypeCommands].Type == blockCommandType)
            {
                numSameTypeCommands++;
            }

            // Write how many commands of the same type we have AKA block size
            writer.Write(numSameTypeCommands);

            // Start writing this block.
            // Iterate over commands and write component hash, then size of the sub-block (number of type methods hashes in this block)
            int blockStartIndex = commandIndex;
            while (commandIndex < blockStartIndex + numSameTypeCommands)
            {
                IRpcSimulationCommand blockCommand = (IRpcSimulationCommand)Commands[commandIndex];

                writer.Write(blockCommand.ComponentTypeHash);

                // Look ahead and count how many back-to-back rpc commands we have with the same component type hash, AKA sub-block size
                byte numSameComponentTypeHashCommands = 1;

                while (commandIndex + numSameComponentTypeHashCommands < Commands.Count
                    && Commands[commandIndex + numSameComponentTypeHashCommands].Type == blockCommandType
                    && ((IRpcSimulationCommand)Commands[commandIndex + numSameComponentTypeHashCommands]).ComponentTypeHash == blockCommand.ComponentTypeHash)
                {
                    numSameComponentTypeHashCommands++;
                }

                // Write number of commands with the same component type hash, aka sub-block size
                writer.Write(numSameComponentTypeHashCommands);

                // Iterate over sub-blocks and write method hashes
                int subBlockStartIndex = commandIndex;
                while(commandIndex < subBlockStartIndex + numSameComponentTypeHashCommands)
                {
                    IRpcSimulationCommand subBlockCommand = (IRpcSimulationCommand)Commands[commandIndex];

                    writer.Write(subBlockCommand.MethodHash);

                    // Look ahead and count how many back-to-back rpc commands we have with the same method hash, aka sub-sub-block size
                    byte numSameComponentTypeHashMethodHashCommands = 1;

                    while (commandIndex + numSameComponentTypeHashMethodHashCommands < Commands.Count
                        && Commands[commandIndex + numSameComponentTypeHashMethodHashCommands].Type == blockCommandType
                        && ((IRpcSimulationCommand)Commands[commandIndex + numSameComponentTypeHashMethodHashCommands]).MethodHash == blockCommand.MethodHash)
                    {
                        numSameComponentTypeHashMethodHashCommands++;
                    }

                    // Write number of commands with the same method hash, aka sub-sub-block size
                    writer.Write(numSameComponentTypeHashMethodHashCommands);

                    // Iterate over sub-sub-blocks and write objectIds (on which this component/method RPC pair was called PLUS values
                    int subSubBlockStartIndex = commandIndex;
                    while(commandIndex < subSubBlockStartIndex + numSameComponentTypeHashMethodHashCommands)
                    {
                        IRpcSimulationCommand subSubBlockCommand = (IRpcSimulationCommand)Commands[commandIndex];
                        
                        // Write object id
                        writer.Write(subSubBlockCommand.ObjectId);

                        // Write RPC parameters
                        var componentType = _simulation.GetComponentTypeByHash(subSubBlockCommand.ComponentTypeHash);
                        var componentWrapper = _simulation.GetComponentWrapper(componentType);

                        ComponentTypeWrapper.RpcParameterInfo[] parameters;
                        if (subSubBlockCommand.Type == CommandType.ServerRpc)
                        {
                            parameters = componentWrapper.GetServerRpcMethodInfo(subSubBlockCommand.MethodHash).Parameters;
                        }
                        else
                        {
                            parameters = componentWrapper.GetClientRpcMethodInfo(subSubBlockCommand.MethodHash).Parameters;
                        }

                        // Write parameters
                        for (int numParam = 0; numParam < parameters.Length; numParam++)
                        {
                            object parameterValue = subSubBlockCommand.Parameters[numParam];
                            Type parameterType = parameters[numParam].ParameterType;

                            // Write
                            writer.Write(parameterValue, parameterType);
                        }

                        commandIndex++;
                    }
                }
            }
        }

        private void WriteStates(IByteStreamWriter writer)
        {
        }
    }
}
