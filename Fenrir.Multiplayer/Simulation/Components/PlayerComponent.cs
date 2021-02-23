﻿using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Events;
using System;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Simulation.Components
{
    public sealed class PlayerComponent : SimulationComponent
    {
        /// <summary>
        /// List of outgoing tick snapshots
        /// </summary>
        private LinkedList<SimulationTickSnapshot> _outgoingTickSnapshots = new LinkedList<SimulationTickSnapshot>();

        /// <summary>
        /// Current tick snapshot
        /// </summary>
        private SimulationTickSnapshot _currentTickSnapshot = null;

        /// <summary>
        /// True if awaiting to send a full snapshot
        /// </summary>
        private bool _fullSnapshotSent = false;

        /// <summary>
        /// Only assigned on the server. 
        /// Used to notify players simulation commands
        /// </summary>
        public IServerPeer ServerPeer { get; set; }

        public PlayerComponent()
        {
        }

        protected override void OnAdded()
        {
            Simulation.CommandCreated += OnCommandCreated;
        }

        protected override void OnBeforeDestroyed()
        {
            Simulation.CommandCreated -= OnCommandCreated;
        }

        protected override void OnDestroyed()
        {
        }

        /// <summary>
        /// Server-side method that removes any snapshots that this client has acknowledged
        /// </summary>
        /// <param name="tickNumber">Removes snapshots with the tick number lower than provided</param>
        public void AcknowledgeTickSnapshot(uint tickNumber)
        {
            while(_outgoingTickSnapshots.First != null)
            {
                if (_outgoingTickSnapshots.First.Value.TickNumber > tickNumber)
                {
                    break; // Subsequent snapshots should be packed and sent unless client acks them
                }
                else
                {
                    // Snapshot was acked, can safely remove
                    _outgoingTickSnapshots.RemoveFirst();
                }
            }
        }

        private void RecycleCurrentTickSnapshot()
        {
            if (_currentTickSnapshot != null)
            {
                _outgoingTickSnapshots.AddLast(_currentTickSnapshot);
            }

            _currentTickSnapshot = new SimulationTickSnapshot(Simulation.CurrentTickNumber, Simulation.CurrentTickTime);

            CompressStateSnapshots();
        }

        private void CompressStateSnapshots()
        {
            // TODO: For each tick snapshot, go over states and remove state updates
            // that update the same value. Only keep the most recent update
        }

        protected override void OnLateTick()
        {
            if (ServerPeer != null)
            {
                // Save current tick snapshot
                RecycleCurrentTickSnapshot();

                // Send outgoing commands to this peer. Keep sending until we get an ACK from the client
                SimulationTickSnapshotEvent tickSnapshotEvent = new SimulationTickSnapshotEvent() { TickSnapshots = _outgoingTickSnapshots }; // TODO: Object pool
                ServerPeer.SendEvent(tickSnapshotEvent, deliveryMethod: MessageDeliveryMethod.Unreliable);
            }
        }

        public void SendSimulationInitEvent()
        {
            if(ServerPeer == null)
            {
                throw new InvalidOperationException("Can not send simulation init event, no server peer assigned (not an authority?)");
            }

            SimulationInitEvent simulationInitEvent = new SimulationInitEvent(Simulation.TickRate, Simulation.CurrentTickTime, GetFullSimulationSnapshot());
            ServerPeer.SendEvent(simulationInitEvent, deliveryMethod: MessageDeliveryMethod.ReliableUnordered);

            _fullSnapshotSent = true;
        }

        private SimulationTickSnapshot GetFullSimulationSnapshot()
        {
            SimulationTickSnapshot snapshot = new SimulationTickSnapshot(Simulation.CurrentTickNumber, Simulation.CurrentTickTime);

            // Generate commands 

            // Get objects
            var simObjects = Simulation.GetObjects();
            foreach(SimulationObject simObject in simObjects)
            {
                SpawnObjectSimulationCommand cmd = new SpawnObjectSimulationCommand(simObject.Id);
                snapshot.Commands.Add(cmd);
            }

            // Get components
            foreach (SimulationObject simObject in simObjects)
            {
                foreach(SimulationComponent component in simObject.GetComponents())
                {
                    AddComponentSimulationCommand cmd = new AddComponentSimulationCommand(simObject.Id, component.TypeHash);
                    snapshot.Commands.Add(cmd);
                }
            }

            // Component states
            // TODO

            return snapshot;
        }

        private void OnCommandCreated(ISimulationCommand command)
        {
            if(!_fullSnapshotSent)
            {
                return; // Waiting for full snapshot to be sent, ignore
            }

            if(_currentTickSnapshot == null)
            {
                _currentTickSnapshot = new SimulationTickSnapshot(Simulation.CurrentTickNumber, Simulation.CurrentTickTime); // TODO: Use object pool
            }
            else if(Simulation.CurrentTickNumber > _currentTickSnapshot.TickNumber)
            {
                // This should not normally happen, this means something went wrong and 
                // simulation tick was advanced in between LateTicks ?
                RecycleCurrentTickSnapshot();
            }

            // Add command to our last snapshot
            _currentTickSnapshot.Commands.Add(command);
        }
    }
}