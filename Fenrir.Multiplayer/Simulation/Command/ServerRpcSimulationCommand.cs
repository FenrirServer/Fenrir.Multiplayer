using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Command
{
    class ServerRpcSimulationCommand : IRpcSimulationCommand
    {
        public CommandType Type => CommandType.ServerRpc;

        public ushort ObjectId { get; private set; }

        public ulong ComponentTypeHash { get; private set; }

        public ulong MethodHash { get; private set; }

        public object[] Parameters { get; private set; }

        public ServerRpcSimulationCommand(ushort objectId, ulong componentTypeHash, ulong methodHash, params object[] parameters)
        {
            ObjectId = objectId;
            ComponentTypeHash = componentTypeHash;
            MethodHash = methodHash;
            Parameters = parameters;
        }
    }
}
