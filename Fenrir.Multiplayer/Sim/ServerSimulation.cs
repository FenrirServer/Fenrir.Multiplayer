using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Sim
{
    public class ServerSimulation : Simulation
    {
        /// <summary>
        /// Simulation server - gets notified of the server simulation events
        /// </summary>
        private ISimulationServerView SimulationServer { get; set; }

        /// <summary>
        /// Player owned objects by server peer. Only available if simulation is runnning on a server.
        /// </summary>
        private Dictionary<IServerPeer, SimulationObject> _players = new Dictionary<IServerPeer, SimulationObject>();

        /// <summary>
        /// True if simulation runs on the host (server)
        /// </summary>
        public override bool IsServer => true;

        /// <summary>
        /// Creates new Server Simulation
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="view">Simulation view, that is being notified of simulation events</param>
        /// <param name="serverView">Simulation view, that is being notified of simulation events</param>
        public ServerSimulation(IFenrirLogger logger, ISimulationView view, ISimulationServerView serverView)
            : base(logger, view)
        {
            if (serverView == null)
            {
                throw new ArgumentNullException(nameof(serverView));
            }

            SimulationServer = serverView;
        }

        #region Player Registration
        public void AddPlayer(IServerPeer serverPeer, string token)
        {
            if (serverPeer == null)
            {
                throw new ArgumentNullException(nameof(serverPeer));
            }

            SimulationObject playerObject = CreateObject();
            _players.Add(serverPeer, playerObject);
            SimulationServer.PlayerJoined(this, playerObject, serverPeer, token);
        }

        public void RemovePlayer(IServerPeer serverPeer)
        {
            if (serverPeer == null)
            {
                throw new ArgumentNullException(nameof(serverPeer));
            }

            if (!_players.ContainsKey(serverPeer))
            {
                throw new Exception($"Can't remove player from Simulation, no player object found for peer {serverPeer}");
            }

            SimulationObject playerObject = _players[serverPeer];
            _players.Remove(serverPeer);

            SimulationServer.PlayerLeft(this, playerObject, serverPeer);
        }
        #endregion

    }
}
