using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.Multiplayer.Sim.Components
{
    public sealed class PlayerComponent : SimulationComponent
    {
        /// <summary>
        /// List of outgoing tick snapshots
        /// </summary>
        private LinkedList<SimulationTickSnapshot> _outgoingTickSnapshots = new LinkedList<SimulationTickSnapshot>();

        /// <summary>
        /// Only assigned on the server. 
        /// Used to notify players simulation commands
        /// </summary>
        public IServerPeer ServerPeer { get; set; }

        /// <summary>
        /// True if player is initialized (first snapshot of the simulation is sent)
        /// </summary>
        private bool _initialized = false;

        public PlayerComponent()
        {
        }

        protected override void OnInitialized()
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

        public void AcknowledgeTickSnapshot(DateTime tickTime)
        {
            while(_outgoingTickSnapshots.First != null)
            {
                if (_outgoingTickSnapshots.First.Value.Time > tickTime)
                {
                    break; // Subsequent commands should be packed and sent unless client acks them
                }
                else
                {
                    // Command was acked, can safely remove
                    _outgoingTickSnapshots.RemoveFirst();
                }
            }
        }

        protected override void OnTick()
        {
            if (ServerPeer != null)
            {
                if(!_initialized)
                {
                    // If first tick, send full snapshot
                    SimulationTickSnapshot simulationTickSnapshot = new SimulationTickSnapshot() { Time = Simulation.Time, Snapshots = GetFullSimulationSnapshot() };
                    SimulationInitEvent simulationInitEvent = new SimulationInitEvent() { SimulationSnapshot = simulationTickSnapshot };
                    _initialized = true;
                }
                else
                {
                    // TODO: For state command, remove per-state duplicates

                    // Send outgoing commands to this peer
                    SimulationTickSnapshotEvent tickSnapshotEvent = new SimulationTickSnapshotEvent() { TickSnapshots = _outgoingTickSnapshots }; // TODO: Object pool
                    ServerPeer.SendEvent(tickSnapshotEvent);
                }
            }
        }

        private Dictionary<CommandType, SimulationCommandListSnapshot> GetFullSimulationSnapshot()
        {
            Dictionary<CommandType, SimulationCommandListSnapshot> commands = new Dictionary<CommandType, SimulationCommandListSnapshot>();

            // Get objects
            SimulationCommandListSnapshot spawnObjectCommandListSnapshot = new SimulationCommandListSnapshot() { CommandType = CommandType.SpawnObject };
            commands.Add(CommandType.SpawnObject, spawnObjectCommandListSnapshot);
            var simObjects = Simulation.GetObjects();
            foreach(SimulationObject simObject in simObjects)
            {
                SpawnObjectSimulationCommand cmd = new SpawnObjectSimulationCommand(Simulation.Time, simObject.Id);
                spawnObjectCommandListSnapshot.Commands.Add(cmd);
            }

            // Get components
            SimulationCommandListSnapshot addComponentCommandListSnapshot = new SimulationCommandListSnapshot() { CommandType = CommandType.AddComponent };
            commands.Add(CommandType.AddComponent, addComponentCommandListSnapshot);
            foreach (SimulationObject simObject in simObjects)
            {
                foreach(SimulationComponent component in simObject.GetComponents())
                {
                    AddComponentSimulationCommand cmd = new AddComponentSimulationCommand(Simulation.Time, simObject.Id, component.TypeHash);
                    addComponentCommandListSnapshot.Commands.Add(cmd);
                }
            }

            // Component states
            // TODO

            return commands;
        }

        private void OnCommandCreated(ISimulationCommand command)
        {
            SimulationTickSnapshot tickSnapshot;

            if (_outgoingTickSnapshots.Last == null || _outgoingTickSnapshots.Last.Value.Time != command.Time)
            {
                tickSnapshot = new SimulationTickSnapshot() { Time = command.Time }; // TODO: Use object pool
                _outgoingTickSnapshots.AddLast(tickSnapshot);
            }
            else
            {
                tickSnapshot = _outgoingTickSnapshots.Last.Value;
            }

            // Add command list snapshot
            if(!tickSnapshot.Snapshots.TryGetValue(command.Type, out SimulationCommandListSnapshot commandListSnapshot))
            {
                commandListSnapshot = new SimulationCommandListSnapshot() { CommandType = command.Type };
                tickSnapshot.Snapshots.Add(command.Type, commandListSnapshot);
            }

            // Add command
            commandListSnapshot.Commands.Add(command);
        }
    }
}
