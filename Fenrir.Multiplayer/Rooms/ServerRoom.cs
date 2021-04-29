﻿using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// True if room action queue is running
        /// </summary>
        protected bool IsRunning => _actionQueue.IsRunning;

        /// <summary>
        /// Peers that joined this room, by peer id
        /// </summary>
        protected Dictionary<string, IServerPeer> Peers = new Dictionary<string, IServerPeer>();

        /// <summary>
        /// Unique room id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Creates Server Room
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="roomId">Unique id</param>
        public ServerRoom(ILogger logger, string roomId)
        {
            Logger = logger;
            _actionQueue = new ActionQueue(logger);
            _actionQueue.Run();
            Id = roomId;
        }

        /// <summary>
        /// Invoked when new peer attempts to join the room. 
        /// Override this method to validate if peer is allowed to join the room
        /// </summary>
        /// <param name="peer">Peer that attempts to join the room</param>
        /// <param name="token">Custom join token provided by the peer</param>
        protected virtual RoomJoinResponse OnBeforePeerJoin(IServerPeer peer, string token)
        {
            return RoomJoinResponse.JoinSuccess;
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
            if (!Peers.ContainsKey(peer.Id))
            {
                throw new InvalidOperationException(string.Format("Failed to remove peer {0} from the room {1}, no such peer", peer.Id, Id));
            }

            Peers.Remove(peer.Id);

            OnPeerLeave(peer);

            if(Peers.Count == 0)
            {
                Terminate();
            }
        }

        /// <summary>
        /// Broadcasts event to all joined peers
        /// </summary>
        /// <typeparam name="TEvent">Type of the event to broadcast</typeparam>
        /// <param name="evt">Event to broadcast</param>
        protected void BroadcastEvent<TEvent>(TEvent evt)
            where TEvent : IEvent
        {
            foreach(var peer in Peers.Values)
            {
                peer.SendEvent(evt);
            }
        }

        /// <summary>
        /// Terminates the room and shuts down room queue.
        /// Invokes <seealso cref="Terminated"/> event
        /// </summary>
        protected virtual void Terminate()
        {
            _actionQueue.Stop();
            Terminated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds callback to the room action queue.
        /// Callbacks will be invoked by the room event loop sequentially, in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <param name="action">Callback</param>
        protected void Execute(Action action)
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
        protected void Schedule(Action action, double delayMs)
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
        /// Adds callback to the room action queue.
        /// Callbacks will be invoked by the room event loop sequentially, in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <param name="callback">Callback to execute</param>
        /// <returns>Task that completes when callback is executed, or fails if callback throws an exception</returns>
        public Task ExecuteAsync(Action callback)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            Execute(() =>
            {
                callback();
                tcs.SetResult(true);
            });

            return tcs.Task;
        }

        /// <summary>
        /// Adds callback to the room action queue.
        /// Callbacks will be invoked by the room event loop sequentially, in a single-threaded manner.
        /// All callbacks invoked on the room event loop are thread-safe and can access 
        /// room resources.
        /// </summary>
        /// <typeparam name="T">Return value type</typeparam>
        /// <param name="callback">Callback to execute</param>
        /// <returns>Task that completes when callback is executed, or fails if callback throws an exception. 
        /// Result of the task contains return value of the callback.</returns>
        public Task<T> ExecuteAsync<T>(Func<T> callback)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            Execute(() =>
            {
                T result = callback();
                tcs.SetResult(result);
            });

            return tcs.Task;
        }

        #region IServerRoom Implementation

        /// <inheritdoc />
        Task<RoomJoinResponse> IServerRoom.AddPeerAsync(IServerPeer peer, string joinToken)
        {
            TaskCompletionSource<RoomJoinResponse> tcs = new TaskCompletionSource<RoomJoinResponse>();

            Execute(() =>
            {
                if (Peers.ContainsKey(peer.Id))
                {
                    return; // Already joined, do nothing
                }

                RoomJoinResponse response = OnBeforePeerJoin(peer, joinToken);

                if (!response.Success)
                {
                    // Failed to join
                    tcs.SetResult(response);
                }
                else
                {
                    // Successfully joined, add peer
                    Peers.Add(peer.Id, peer);
                    OnPeerJoin(peer, joinToken);

                    tcs.SetResult(response);
                }
            });

            return tcs.Task;
        }

        /// <inheritdoc />
        Task<RoomLeaveResponse> IServerRoom.RemovePeerAsync(IServerPeer peer)
        {
            TaskCompletionSource<RoomLeaveResponse> tcs = new TaskCompletionSource<RoomLeaveResponse>();

            Execute(() =>
            {
                RemovePeer(peer);
                tcs.SetResult(RoomLeaveResponse.LeaveSuccess);
            });

            return tcs.Task;
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
