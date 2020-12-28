using Fenrir.Multiplayer.Network;
using System;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Server Room
    /// Rooms allow you to build an isolated layer of gameplay and
    /// business logic using single-threaded event loop, and benefit
    /// from multi-threaded architecture where each server can handle thousands of players.
    /// </summary>
    public interface IServerRoom : IDisposable
    {
        /// <summary>
        /// Invoked when room is terminated (e.g. last peer leaves)
        /// </summary>
        event EventHandler Terminated;

        /// <summary>
        /// Unique room id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Adds peer to the room
        /// </summary>
        /// <param name="peer">Peer</param>
        /// <param name="token">Join Token</param>
        void AddPeer(IServerPeer peer, string token);

        /// <summary>
        /// Removes peer from the room
        /// </summary>
        /// <param name="peer">Peer</param>
        void RemovePeer(IServerPeer peer);
    }
}