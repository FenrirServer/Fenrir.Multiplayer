using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Sim.Requests;

namespace Fenrir.Multiplayer.Sim
{
    public class SimulationRoomManager : ServerRoomManager<SimulationRoom>
        , IRequestHandler<SimulationTickSnapshotAckRequest>
    {
        public SimulationRoomManager(IFenrirLogger logger) : base(logger)
        {
        }

        protected override SimulationRoom CreateRoom(IServerPeer peer, string roomId, string token)
        {
            return new SimulationRoom(Logger, roomId);
        }

        public void HandleRequest(SimulationTickSnapshotAckRequest request, IServerPeer peer)
        {
            if(peer.PeerData == null)
            {
                Logger.Warning($"Failed to handle {nameof(SimulationTickSnapshotAckRequest)}: peer not in a room");
                return;
            }

            SimulationRoom room = (SimulationRoom)peer.PeerData;

            room.AcknowledgeTickSnapshot(peer, request.TickTime);
        }
    }
}
