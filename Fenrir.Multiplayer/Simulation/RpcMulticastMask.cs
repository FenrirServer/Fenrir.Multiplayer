using System;

namespace Fenrir.Multiplayer.Simulation
{
    /// <summary>
    /// Client RPC multicast mask
    /// Determines who receives the RPC call
    /// </summary>
    [Flags]
    public enum RpcMulticastMask : byte
    {
        /// <summary>
        /// RPC is not invoked on clients
        /// </summary>
        None = 0,

        /// <summary>
        /// RPC is invoked on the server
        /// </summary>
        Server = 1,

        /// <summary>
        /// RPC is invoked on the client who owns the object
        /// </summary>
        Owner = 2,

        /// <summary>
        /// RPC is invoked on clients that have assigned group
        /// </summary>
        Group = 4,
        
        /// <summary>
        /// RPC is invoked on all players and the server
        /// </summary> // All players get this RPC - default?
        All = 255,
    }
}
