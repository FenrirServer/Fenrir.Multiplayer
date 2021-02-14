using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Components;
using Fenrir.Multiplayer.Sim.Data;
using Fenrir.Multiplayer.Sim.Events;
using Fenrir.Multiplayer.Sim.Requests;
using Fenrir.Multiplayer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Sim
{
    public class SimulationClient
        : IEventHandler<SimulationInitEvent>
        , IEventHandler<SimulationTickSnapshotEvent>
        , IEventHandler<SimulationClockSyncAckEvent>
    {
        /// <summary>
        /// Fenrir client
        /// </summary>
        private readonly IFenrirClient _client;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly IFenrirLogger _logger;

        /// <summary>
        /// Simulation
        /// </summary>
        public Simulation Simulation { get; private set; }

        /// <summary>
        /// Simulation tick rate, how many ticks per second
        /// </summary>
        public int TickRate { get; set; } = 66;

        /// <summary>
        /// Number of initial clock synchronization request
        /// </summary>
        public int NumInitialClockSyncRequests { get; set; } = 5;

        /// <summary>
        /// Delay between initial clock sync requests
        /// </summary>
        public double InitialClockSyncDelayMs { get; set; } = 5;

        /// <summary>
        /// Stopwatch used to tick simulation
        /// </summary>
        private readonly Stopwatch _simulationTickStopwatch = new Stopwatch();

        /// <summary>
        /// Clock synchronizer - keeps track of the clock
        /// offset between client and server
        /// </summary>
        private readonly ClockSynchronizer _clockSynchronizer;

        /// <summary>
        /// Current room id
        /// </summary>
        private string _roomId = null;

        /// <summary>
        /// True if join simulation room
        /// </summary>
        private bool _isJoined => _roomId != null;

        /// <summary>
        /// True if simulation is running
        /// </summary>
        private bool _isRunningSimulation = false;

        /// <summary>
        /// Task completion source, completes when
        /// simulation synchronization is completed.
        /// This happens when client simulation processes 
        /// first server tick snapshot
        /// </summary>
        private TaskCompletionSource<bool> _firstSnapshotTcs = null;


        public SimulationClient(IFenrirClient client, IFenrirLogger logger)
        {
            _client = client;
            _logger = logger;

            _client.AddEventHandler<SimulationInitEvent>(this);
            _client.AddEventHandler<SimulationTickSnapshotEvent>(this);
            _client.AddEventHandler<SimulationClockSyncAckEvent>(this);
            _client.AddSerializableTypeFactory<SimulationInitEvent>(() => new SimulationInitEvent(Simulation));
            _client.AddSerializableTypeFactory<SimulationTickSnapshotEvent>(() => new SimulationTickSnapshotEvent(Simulation));

            _client.Disconnected += OnDisconnected;

            Simulation = new Simulation(logger) { IsAuthority = false };
            Simulation.TickSnapshotProcessed += OnTickSnapshotProcessed;

            _clockSynchronizer = new ClockSynchronizer();

            RegisterBuiltInSimulationComponents();
        }


        private void RegisterBuiltInSimulationComponents()
        {
            Simulation.RegisterComponentType<PlayerComponent>();
        }

        public async Task<SimulationJoinResult> Join(string roomId, string joinToken)
        {
            if(_isJoined)
            {
                throw new InvalidOperationException("Can't join simulation, already joined one");
            }

            if(_client.State != ConnectionState.Connected)
            {
                return new SimulationJoinResult(-1, "Client is not connected");
            }

            // Synchronize simulation clock
            await SyncClockInit();

            // Join simulation room
            var joinRequest = new RoomJoinRequest(roomId, joinToken);
            var joinResponse = await _client.Peer.SendRequest<RoomJoinRequest, RoomJoinResponse>(joinRequest);

            // Failed to join
            if(!joinResponse.Success)
            {
                return new SimulationJoinResult(joinResponse);
            }

            _roomId = roomId;

            // Successfully joined simulation. Let's wait until we receive Simulation init event before running the simulation
            _firstSnapshotTcs = new TaskCompletionSource<bool>();
            await _firstSnapshotTcs.Task; // Wait until simulation sync-up is completed

            return new SimulationJoinResult(joinResponse);
        }


        public async Task Leave()
        {
            if (!_isJoined)
            {
                throw new InvalidOperationException("Can't leave simulation, not in a simulation");
            }

            if (_client.State != ConnectionState.Connected)
            {
                return; // Disconnected, already left
            }


            var leaveRequest = new RoomLeaveRequest();
            RoomLeaveResponse response = await _client.Peer.SendRequest<RoomLeaveRequest, RoomLeaveResponse>(leaveRequest);

            StopSimulation();
        }

        private void OnDisconnected(object sender, Multiplayer.Events.DisconnectedEventArgs e)
        {
            StopSimulation();
        }

        private void StopSimulation()
        {
            _isRunningSimulation = false;
            _roomId = null;
            _firstSnapshotTcs = null;
        }

        private async void RunSimulation()
        {
            _isRunningSimulation = true;

            while (_isRunningSimulation)
            {
                // Check if we need to sync simulation clock
                if (DateTime.UtcNow > _clockSynchronizer.NextSyncTime)
                {
                    SendSyncClockRequest().FireAndForget(_logger);
                }

                // Tick simulation
                await TickSimulation();
            }
        }

        private async Task TickSimulation()
        {
            _simulationTickStopwatch.Start();

            double delayBetweenTicksMs = 1000d / TickRate;

            try
            {
                Simulation.Tick();
            }
            catch (Exception e)
            {
                _logger.Error("Error during simulation tick: {0}", e.ToString());
            }

            _simulationTickStopwatch.Stop();

            double timeElapsedMs = (double)_simulationTickStopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond;

            _simulationTickStopwatch.Reset();

            if (timeElapsedMs > delayBetweenTicksMs)
            {
                _logger.Warning("Simulation tick took {0} milliseconds, which is longer than tick rate {1} milliseconds. Scheduling next tick immediately", timeElapsedMs, delayBetweenTicksMs);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayBetweenTicksMs - timeElapsedMs));
            }
        }


        private void IngestTickSnapshot(SimulationTickSnapshot tickSnapshot)
        {
            // Ingest initial commands
            Simulation.IngestTickSnapshot(tickSnapshot);
        }

        private void OnTickSnapshotProcessed(SimulationTickSnapshot tickSnapshot)
        {
            if (_isJoined && _firstSnapshotTcs != null)
            {
                // First command ever, assume simulation has finished initialization
                _firstSnapshotTcs?.SetResult(true);
                _firstSnapshotTcs = null;
            }

            Simulation.TickSnapshotProcessed -= OnTickSnapshotProcessed;
        }

        #region Clock Sync
        private async Task SyncClockInit()
        {
            await Task.WhenAll(SyncSimulationClockTasks());

            // Set offset
            Simulation.SetClockOffset(_clockSynchronizer.AvgOffset);
        }

        private IEnumerable<Task> SyncSimulationClockTasks()
        {
            for (int i = 0; i < NumInitialClockSyncRequests; i++)
            {
                yield return SendSyncClockRequest(i * InitialClockSyncDelayMs);
            }
        }

        private async Task SendSyncClockRequest(double delayMs = 0)
        {
            if (delayMs > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs));
            }

            var simClockSyncRequest = new SimulationClockSyncRequest(DateTime.UtcNow);

            _client.Peer?.SendRequest<SimulationClockSyncRequest>(simClockSyncRequest, 0, MessageDeliveryMethod.Unreliable);
        }
        #endregion

        #region Event Handlers

        void IEventHandler<SimulationClockSyncAckEvent>.OnReceiveEvent(SimulationClockSyncAckEvent evt)
        {
            DateTime timeReceivedResponse = DateTime.UtcNow;

            // Record clock synchronization data
            _clockSynchronizer.RecordSyncResult(evt.TimeSentRequest, evt.TimeReceivedRequest, evt.TimeSentResponse, timeReceivedResponse);
        }

        void IEventHandler<SimulationInitEvent>.OnReceiveEvent(SimulationInitEvent evt)
        {
            if(!_isJoined)
            {
                return;
            }

            // Apply snapshot
            IngestTickSnapshot(evt.SimulationSnapshot);

            // Start ticking
            RunSimulation();
        }

        void IEventHandler<SimulationTickSnapshotEvent>.OnReceiveEvent(SimulationTickSnapshotEvent evt)
        {
            // Dispatch tick snapshots
            foreach(SimulationTickSnapshot snapshot in evt.TickSnapshots)
            {
                IngestTickSnapshot(snapshot);
            }

            // Acknowledge tick snapshot
            SimulationTickSnapshotAckRequest ackRequest = new SimulationTickSnapshotAckRequest(evt.TickSnapshots.Last.Value.TickTime);
            _client.Peer.SendRequest(ackRequest, deliveryMethod: MessageDeliveryMethod.Unreliable);
        }
        #endregion
    }
}
