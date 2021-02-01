using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Components;
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
        /// simulation synchronization is completed
        /// </summary>
        private TaskCompletionSource<bool> _initTcs = null;


        public SimulationClient(IFenrirClient client, IFenrirLogger logger)
        {
            _client = client;
            _logger = logger;

            _client.AddEventHandler<SimulationInitEvent>(this);
            _client.AddEventHandler<SimulationTickSnapshotEvent>(this);
            _client.AddEventHandler<SimulationClockSyncAckEvent>(this);

            _client.Disconnected += OnDisconnected;

            Simulation = new Simulation(logger) { IsAuthority = false };
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
            _initTcs = new TaskCompletionSource<bool>();
            await _initTcs.Task; // Wait until simulation sync-up is completed

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
            _initTcs = null;
        }

        private async void RunSimulationTickLoop()
        {
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


        private void ApplyTickSnapshot(SimulationTickSnapshot tickSnapshot)
        {
            // Ingest initial commands
            foreach(SimulationCommandListSnapshot commandListSnapshot in tickSnapshot.Snapshots.Values)
            {
                foreach(ISimulationCommand command in commandListSnapshot.Commands)
                {
                    Simulation.IngestCommand(command);
                }
            }
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
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));

            var simClockSyncRequest = new SimulationClockSyncRequest(DateTime.UtcNow);

            _client.Peer.SendRequest<SimulationClockSyncRequest>(simClockSyncRequest, 0, MessageDeliveryMethod.Unreliable);
        }
        #endregion

        #region Event Handlers

        void IEventHandler<SimulationClockSyncAckEvent>.OnReceiveEvent(SimulationClockSyncAckEvent evt)
        {
            DateTime timeReceivedResponse = DateTime.UtcNow;

            // Record clock synchronization data
            _clockSynchronizer.RecordSyncResult(evt.TimeSentRequest, evt.TimeReceivedRequest, evt.TimeSentResponse, timeReceivedResponse);
        }

        async void IEventHandler<SimulationInitEvent>.OnReceiveEvent(SimulationInitEvent evt)
        {
            if(!_isJoined)
            {
                return;
            }

            // Apply snapshot
            ApplyTickSnapshot(evt.SimulationSnapshot);

            // Wait until we buffer simulation ticks until the first command is executed
            // TODO: Maybe use Simulation.CommandCreated to detect first ever command ?
            await Task.Delay(TimeSpan.FromMilliseconds(Simulation.IncomingCommandDelayMs));

            if (!_isJoined)
            {
                return;
            }

            _initTcs?.SetResult(true);
            _isRunningSimulation = true;

            // Start ticking
            RunSimulationTickLoop();
        }

        void IEventHandler<SimulationTickSnapshotEvent>.OnReceiveEvent(SimulationTickSnapshotEvent evt)
        {
            // Dispatch tick snapshots
            foreach(SimulationTickSnapshot snapshot in evt.TickSnapshots)
            {
                ApplyTickSnapshot(snapshot);
            }
        }
        #endregion
    }
}
