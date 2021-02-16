using System;

namespace Fenrir.Multiplayer.Simulation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
        public RpcMulticastMask Multicast;

        public byte MulticastGroup;

        public ClientRpcAttribute(RpcMulticastMask multicast = RpcMulticastMask.All ^ RpcMulticastMask.Server, byte multicastGroup = 0)
        {
            Multicast = multicast;
            MulticastGroup = multicastGroup;
        }
    }
}