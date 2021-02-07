﻿using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Server Room Manager - provides simple way for users to leave and join rooms.
    /// If room with a given id does not exist, it is created.
    /// When last person leaves the room it is automatically destroyed.
    /// </summary>
    /// <typeparam name="TRoom">Type of room</typeparam>
    public class ServerRoomManager<TRoom>
        : IRequestHandler<RoomJoinRequest, RoomJoinResponse>
        , IRequestHandler<RoomLeaveRequest, RoomLeaveResponse>
        where TRoom : IServerRoom
    {

        /// <summary>
        /// Delegate for room creation. Invoked when new room needs to be created
        /// </summary>
        /// <param name="peer">Peer that creates a room</param>
        /// <param name="roomId">Room id</param>
        /// <param name="joinToken">(optional) custom token supplied by the peer</param>
        /// <returns>Newly created room</returns>
        public delegate TRoom CreateRoomHandler(IServerPeer peer, string roomId, string joinToken);

        /// <summary>
        /// Logger
        /// </summary>
        protected IFenrirLogger Logger { get; private set; }
        
        /// <summary>
        /// Fenrir server
        /// </summary>
        public IFenrirServer Server { get; private set; }

        /// <summary>
        /// Room creation callback
        /// </summary>
        private readonly CreateRoomHandler _roomFactoryMethod = null;

        /// <summary>
        /// Server room by room id
        /// </summary>
        private readonly Dictionary<string, TRoom> _rooms = new Dictionary<string, TRoom>();

        /// <summary>
        /// Creates room manager
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="server">Server</param>
        protected ServerRoomManager(IFenrirLogger logger, IFenrirServer server)
        {
            Logger = logger;
            Server = server;

            Server.AddRequestHandler<RoomJoinRequest, RoomJoinResponse>(this);
            Server.AddRequestHandler<RoomLeaveRequest, RoomLeaveResponse>(this);
        }

        /// <summary>
        /// Creates server room manager with a given room factory.
        /// Implement IServerRoomFactory interface for custom room creation
        /// </summary>
        /// <param name="roomFactory">Room factory that creates a room of a type <seealso cref="TRoom"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="server">Server</param>
        public ServerRoomManager(IServerRoomFactory<TRoom> roomFactory, IFenrirLogger logger, IFenrirServer server)
            : this(logger, server)
        {
            if(roomFactory == null)
            {
                throw new ArgumentNullException(nameof(roomFactory));
            }

            _roomFactoryMethod = roomFactory.Create;
        }

        /// <summary>
        /// Creates server room manager with a given room factory method.
        /// Pass in a callback that creates new room, e.g. new ServerRoomManager(() => new MyRoom(logger, ...))
        /// </summary>
        /// <param name="roomFactoryMethod">Factory method that creates new room of type <seealso cref="TRoom"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="server">Server</param>
        public ServerRoomManager(CreateRoomHandler roomFactoryMethod, IFenrirLogger logger, IFenrirServer server)
            : this(logger, server)
        {
            if (roomFactoryMethod == null)
            {
                throw new ArgumentNullException(nameof(roomFactoryMethod));
            }

            _roomFactoryMethod = roomFactoryMethod;
        }

        /// <summary>
        /// Returns all rooms
        /// </summary>
        /// <returns></returns>
        internal TRoom[] GetRooms()
        {
            lock(_rooms)
            {
                return _rooms.Values.ToArray();
            }
        }

        private bool TryGetOrCreateRoom(IServerPeer peer, string roomId, string token, out TRoom room)
        {
            room = default;

            // Try to get a room
            lock (_rooms)
            {
                if (_rooms.TryGetValue(roomId, out room))
                {
                    return true;
                }

                // Try to create a room
                if (!TryCreateRoom(peer, roomId, token, out room))
                {
                    return false; // Failed to create a room
                }

                // Room was created
                _rooms.Add(roomId, room);

                return true;
            }
        }

        private bool TryCreateRoom(IServerPeer peer, string roomId, string token, out TRoom room)
        {
            room = default;

            try
            {
                room = CreateRoom(peer, roomId, token);
            }
            catch(Exception e)
            {
                Logger.Error("Uncaught exception during room creation with id {0}: {1}", roomId, e.ToString());
                return false;
            }

            if(room == null)
            {
                Logger.Error("Failed to create a room with id {0}, room factory returned null", roomId);
                return false;
            }

            room.Terminated += OnRoomTerminated;
            return true;
        }

        protected virtual TRoom CreateRoom(IServerPeer peer, string roomId, string token)
        {
            return _roomFactoryMethod(peer, roomId, token);
        }

        private void OnRoomTerminated(object sender, EventArgs e)
        {
            TRoom room;
            try
            {
                room = (TRoom)sender;
            }
            catch(InvalidCastException ex)
            {
                Logger.Error("Failed to terminate room, failed to cast {0} to {1}: {2}", sender.GetType().Name, typeof(TRoom).Name, ex.ToString());
                return;
            }

            room.Terminated -= OnRoomTerminated;

            lock (_rooms)
            {
                _rooms.Remove(room.Id);
            }

            room.Dispose();
        }

        #region Request Handler Implementation
        RoomJoinResponse IRequestHandler<RoomJoinRequest, RoomJoinResponse>.HandleRequest(RoomJoinRequest request, IServerPeer peer)
        {
            if(!TryGetOrCreateRoom(peer, request.RoomId ?? Guid.NewGuid().ToString(), request.Token, out TRoom room))
            {
                return RoomJoinResponse.JoinFailed;
            }

            // Join first user
            room.AddPeer(peer, request.Token);
            peer.PeerData = room;

            return RoomJoinResponse.JoinSuccess;
        }

        RoomLeaveResponse IRequestHandler<RoomLeaveRequest, RoomLeaveResponse>.HandleRequest(RoomLeaveRequest request, IServerPeer peer)
        {
            if(request.RoomId == null)
            {
                Logger.Warning("Failed to remove peer {0} from the room, {1} is null", peer.EndPoint, nameof(request.RoomId));
                return RoomLeaveResponse.LeaveFailed;
            }

            TRoom room;

            lock(_rooms)
            {
                if(!_rooms.TryGetValue(request.RoomId, out room))
                {
                    return RoomLeaveResponse.LeaveFailed;
                }
            }

            peer.PeerData = null;
            room.RemovePeer(peer);

            return RoomLeaveResponse.LeaveSuccess;
        }
        #endregion
    }
}
