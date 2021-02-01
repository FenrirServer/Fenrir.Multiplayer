using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Sim.Events;
using Fenrir.Multiplayer.Sim.Requests;
using System;

namespace Fenrir.Multiplayer.Sim
{
    public class SimulationRoomManager : ServerRoomManager<SimulationRoom>
        , IRequestHandler<SimulationTickSnapshotAckRequest>
        , IRequestHandler<SimulationClockSyncRequest>
    {
        public SimulationRoomManager(IFenrirLogger logger, IFenrirServer server) : base(logger, server)
        {
            server.AddRequestHandler<SimulationClockSyncRequest>(this);
            server.AddRequestHandler<SimulationTickSnapshotAckRequest>(this);
        }

        protected override SimulationRoom CreateRoom(IServerPeer peer, string roomId, string token)
        {
            return new SimulationRoom(Logger, roomId);
        }

        void IRequestHandler<SimulationTickSnapshotAckRequest>.HandleRequest(SimulationTickSnapshotAckRequest request, IServerPeer peer)
        {
            if(peer.PeerData == null)
            {
                Logger.Warning($"Failed to handle {nameof(SimulationTickSnapshotAckRequest)}: peer not in a room");
                return;
            }

            SimulationRoom room = (SimulationRoom)peer.PeerData;

            room.AcknowledgeTickSnapshot(peer, request.TickTime);
        }

        void IRequestHandler<SimulationClockSyncRequest>.HandleRequest(SimulationClockSyncRequest request, IServerPeer peer)
        {
            DateTime requestReceivedTime = DateTime.UtcNow;

            // Respond with an ack
            var simulationClockSyncAckEvent = new SimulationClockSyncAckEvent(request.RequestSentTime, requestReceivedTime);
            peer.SendEvent(simulationClockSyncAckEvent);
        }
    }
}
