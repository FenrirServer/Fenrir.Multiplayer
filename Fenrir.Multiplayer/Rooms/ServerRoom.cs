using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Base class for a Fenrir Multiplayer Room.
    /// Rooms allow you to build an isolated layer of gameplay and
    /// business logic using single-threaded event loop, and benefit
    /// from multi-threaded architecture where each server can handle thousands of players.
    /// </summary>
    public abstract class ServerRoom : IServerRoom
    {
        /// <summary>
        /// Invoked when room is terminated (e.g. last peer leaves)
        /// </summary>
        public event EventHandler Terminated;

        /// <summary>
        /// Room action queue
        /// </summary>
        private ActionQueue _actionQueue;

        /// <summary>
        /// Room Logger
        /// </summary>
        protected IFenrirLogger Logger { get; private set; }

        /// <summary>
        /// True if room action queue is running
        /// </summary>
        protected bool IsRunning => _actionQueue.IsRunning;

        /// <summary>
        /// Unique room id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Peers that joined this room, by peer id
        /// </summary>
        private Dictionary<string, IServerPeer> _peers = new Dictionary<string, IServerPeer>();


        /// <summary>
        /// Creates Server Room
        /// </summary>
        /// <param name="logger">Logger</param>
        public ServerRoom(IFenrirLogger logger)
        {
            Logger = logger;
            _actionQueue = new ActionQueue(logger);
            _actionQueue.Run();
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Adds callback to the room action queue.
        /// Callback will be invoked by the room event loop in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <param name="action">Callback</param>
        protected void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Schedules callback to be invoked on the room action queue, with a given delay.
        /// Callback will be invoked by the room event loop in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="delayMs">Delay after which callback will be invoked</param>
        protected void Schedule(Action action, int delayMs)
        {
            _actionQueue.Schedule(action, delayMs);
        }

        /// <summary>
        /// Schedules callback to be invoked on the room action queue, with a given delay.
        /// Callback will be invoked by the room event loop in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="delay">Delay after which callback will be invoked</param>
        protected void Schedule(Action action, TimeSpan delay)
        {
            _actionQueue.Schedule(action, delay);
        }

        /// <summary>
        /// Invoked when new peer joins the room
        /// </summary>
        /// <param name="peer">Peer</param>
        protected abstract void OnPeerJoin(IServerPeer peer, string token);

        /// <summary>
        /// Invoked when peer leves the room
        /// </summary>
        /// <param name="peer">Peer</param>
        protected abstract void OnPeerLeave(IServerPeer peer);

        /// <summary>
        /// Removes peer from the room
        /// </summary>
        /// <param name="peer">Peer</param>
        protected void RemovePeer(IServerPeer peer)
        {
            if (!_peers.ContainsKey(peer.Id))
            {
                throw new InvalidOperationException(string.Format("Failed to remove peer {0} from the room {1}, no such peer", peer.Id, Id));
            }

            _peers.Remove(peer.Id);

            OnPeerLeave(peer);

            if(_peers.Count == 0)
            {
                Terminate();
            }
        }

        protected void BroadcastEvent<TEvent>(TEvent evt)
            where TEvent : IEvent
        {
            foreach(var peer in _peers.Values)
            {
                peer.SendEvent(evt);
            }
        }

        protected virtual void Terminate()
        {
            _actionQueue.Stop();
            Terminated?.Invoke(this, EventArgs.Empty);
        }

        #region IServerRoom Implementation
        void IServerRoom.AddPeer(IServerPeer peer, string joinToken)
        {
            Enqueue(() =>
            {
                if (_peers.ContainsKey(peer.Id))
                {
                    return; // Already joined, do nothing
                }

                _peers.Add(peer.Id, peer);
                OnPeerJoin(peer, joinToken);
            });
        }

        void IServerRoom.RemovePeer(IServerPeer peer)
        {
            Enqueue(() => RemovePeer(peer));
        }


        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            if(IsRunning)
            {
                Terminate();
            }

            _actionQueue.Dispose();
        }
        #endregion
    }
}
