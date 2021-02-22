using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Simulation.Events;
using Fenrir.Multiplayer.Simulation.Requests;
using System;

namespace Fenrir.Multiplayer.Simulation
{
    public class SimulationRoomManager<TRoom> : ServerRoomManager<TRoom>
        , IRequestHandler<SimulationTickSnapshotAckRequest>
        , IRequestHandler<SimulationClockSyncRequest>
        where TRoom : SimulationRoom
    {
        /// <summary>
        /// Creates simulation room manager with a given room factory.
        /// Implement IServerRoomFactory interface for custom room creation
        /// </summary>
        /// <param name="roomFactory">Room factory that creates a room of a type <seealso cref="TRoom"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="server">Server</param>
        public SimulationRoomManager(IServerRoomFactory<TRoom> roomFactory, ILogger logger, INetworkServer server)
            : base(roomFactory, logger, server)
        {
            RegisterRequestHandlers(server);
        }

        /// <summary>
        /// Creates simulation room manager with a given room factory method.
        /// Pass in a callback that creates new room, e.g. new ServerRoomManager(() => new SimulationRoom(logger, ...))
        /// </summary>
        /// <param name="roomFactoryMethod">Factory method that creates new room of type <seealso cref="TRoom"/></param>
        /// <param name="logger">Logger</param>
        /// <param name="server">Server</param>
        public SimulationRoomManager(CreateRoomHandler roomFactoryMethod, ILogger logger, INetworkServer server)
            : base(roomFactoryMethod, logger, server)
        {
            RegisterRequestHandlers(server);
        }

        /// <summary>
        /// Registers request handlers with the server
        /// </summary>
        /// <param name="server">Network Server</param>
        private void RegisterRequestHandlers(INetworkServer server)
        {
            server.AddRequestHandler<SimulationTickSnapshotAckRequest>(this);
            server.AddRequestHandler<SimulationClockSyncRequest>(this);
        }

        #region Request Handlers
        void IRequestHandler<SimulationTickSnapshotAckRequest>.HandleRequest(SimulationTickSnapshotAckRequest request, IServerPeer peer)
        {
            if(peer.PeerData == null)
            {
                Logger.Warning($"Failed to handle {nameof(SimulationTickSnapshotAckRequest)}: peer not in a room");
                return;
            }

            SimulationRoom room = (SimulationRoom)peer.PeerData;

            room.AcknowledgeTickSnapshot(peer, request.TickNumber);
        }

        void IRequestHandler<SimulationClockSyncRequest>.HandleRequest(SimulationClockSyncRequest request, IServerPeer peer)
        {
            DateTime requestReceivedTime = DateTime.UtcNow;

            // Respond with an ack
            var simulationClockSyncAckEvent = new SimulationClockSyncAckEvent(request.RequestSentTime, requestReceivedTime);
            peer.SendEvent(simulationClockSyncAckEvent);
        }
        #endregion
    }
}
