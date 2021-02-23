using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Simulation.Components;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Events;
using Fenrir.Multiplayer.Simulation.Requests;
using Fenrir.Multiplayer.Utility;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Simulation
{
    public class SimulationClient
        : IEventHandler<SimulationInitEvent>
        , IEventHandler<SimulationTickSnapshotEvent>
        , IEventHandler<SimulationClockSyncAckEvent>
        , IDisposable
    {
        /// <summary>
        /// Network client
        /// </summary>
        private readonly INetworkClient _client;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Simulation
        /// </summary>
        public NetworkSimulation Simulation { get; private set; }

        /// <summary>
        /// Initial clock synchronization timeout
        /// </summary>
        public TimeSpan InitialClockSyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Number of initial clock synchronization events, 
        /// that have to be received in order to start a simulation
        /// </summary>
        public int NumInitialClockSyncEvents { get; set; } = 5;

        /// <summary>
        /// Delay between initial clock sync requests.
        /// This is a magically selected number, based on the assumption that avg player
        /// will have a ping somewhere between 10ms and 100ms.
        /// It will take 50ms to send all pings out, and we are very likely to start receiving some acks by then
        /// </summary>
        public double InitialClockSyncDelayMs { get; set; } = 10;

        /// <summary>
        /// Smooth clock correction step size
        /// </summary>
        public TimeSpan SmoothClockOffsetCorrectionStepSize { get; set; } = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Threshold after which we should just snap the offset instead of applying smooth correction
        /// </summary>
        public TimeSpan SmoothClockOffsetThreshold { get; set; } = TimeSpan.FromMilliseconds(200);

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
        /// Last known server tick number
        /// </summary>
        private uint _lastKnownServerTickNumber;

        /// <summary>
        /// Last known server tick time
        /// </summary>
        private DateTime _lastKnownServerTickTime;

        /// <summary>
        /// Locking primitive for last known tick info
        /// </summary>
        private object _lastKnownTickSyncRoot = new object();

        /// <summary>
        /// Target simulation clock offset
        /// </summary>
        private TimeSpan _targetClockOffset;

        /// <summary>
        /// Delay of the client simulation
        /// Currently calculated as a half RTT + a buffer (single tick)
        /// TODO: Change buffer dynamically based on input LOSS
        /// </summary>
        private TimeSpan _simulationTimeOffset => new TimeSpan(_clockSynchronizer.AvgRoundTripTime.Ticks / 2) + Simulation.TimePerTick;

        /// <summary>
        /// Clock synchronization task completion source.
        /// Completes when initial clock synchronization is done
        /// </summary>
        private TaskCompletionSource<bool> _clockSyncInitTcs = null;

        /// <summary>
        /// Task completion source, completes when
        /// simulation synchronization is completed.
        /// This happens when client simulation processes 
        /// first server tick snapshot
        /// </summary>
        private TaskCompletionSource<bool> _firstSnapshotDispatchedTcs = null;


        public SimulationClient(INetworkClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;

            // Add network event handlers
            _client.AddEventHandler<SimulationInitEvent>(this);
            _client.AddEventHandler<SimulationTickSnapshotEvent>(this);
            _client.AddEventHandler<SimulationClockSyncAckEvent>(this);

            // Add event listeners
            _client.Disconnected += OnDisconnected;

            Simulation = new NetworkSimulation(logger) { IsAuthority = false };
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
            await SyncSimulationClock().TimeoutAfter(InitialClockSyncTimeout);

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
            _firstSnapshotDispatchedTcs = new TaskCompletionSource<bool>();
            await _firstSnapshotDispatchedTcs.Task; // Wait until simulation sync-up is completed. 

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
            _firstSnapshotDispatchedTcs = null;
            _clockSyncInitTcs = null;
        }

        private async void RunSimulation()
        {
            _isRunningSimulation = true;

            while (_isRunningSimulation)
            {
                // Check if we need to sync simulation clock
                if (DateTime.UtcNow > _clockSynchronizer.NextSyncTime)
                {
                    SendSyncClockRequest();
                }

                // Correct clock
                ApplySimulationClockSmoothCorrection();

                // Tick simulation
                await TickSimulation();
            }
        }

        private async Task TickSimulation()
        {
            // Calculate delay until next simulation tick
            TimeSpan nextTickDelay = GetNextSimulationTickTime() - Simulation.ClockTime;

            // If next tick is in the future, wait until and tick, otherwise tick immediately to catch-up
            if(nextTickDelay > TimeSpan.Zero)
            {
                await Task.Delay(nextTickDelay);
            }

            // Tick
            try
            {
                Simulation.Tick();
            }
            catch (Exception e)
            {
                _logger.Error("Error during simulation tick: {0}", e.ToString());
            }
        }

        private DateTime GetNextSimulationTickTime()
        {
            uint numNextTick = Simulation.CurrentTickNumber + 1; // Number of the next expected tick

            DateTime nextTickTime;

            lock (_lastKnownTickSyncRoot)
            {
                uint deltaTicks = numNextTick - _lastKnownServerTickNumber; // Number of ticks between the last known server tick and the next tick
                TimeSpan deltaTime = TimeSpan.FromMilliseconds(Simulation.TimePerTick.TotalMilliseconds * deltaTicks); // Time between last known server tick and the next tick
                nextTickTime = _lastKnownServerTickTime + deltaTime; // Absolute server time of the next tick
            }

            return nextTickTime;
        }

        private void IngestTickSnapshot(SimulationTickSnapshot tickSnapshot)
        {
            // Ingest initial commands
            Simulation.IngestTickSnapshot(tickSnapshot);
        }

        private void OnTickSnapshotProcessed(SimulationTickSnapshot tickSnapshot)
        {
            if (_isJoined && _firstSnapshotDispatchedTcs != null)
            {
                // First command ever, assume simulation has finished initialization
                _firstSnapshotDispatchedTcs?.SetResult(true);
                _firstSnapshotDispatchedTcs = null;
            }

            Simulation.TickSnapshotProcessed -= OnTickSnapshotProcessed;
        }

        #region Clock Sync
        private async Task SyncSimulationClock()
        {
            _clockSyncInitTcs = new TaskCompletionSource<bool>();

            // Keep sending those until initial synchronization is done
            while (_clockSyncInitTcs != null)
            {
                SendSyncClockRequest();
                await Task.WhenAny(_clockSyncInitTcs.Task, Task.Delay(TimeSpan.FromMilliseconds(InitialClockSyncDelayMs)));
            }
        }


        private void SendSyncClockRequest()
        {
            var simClockSyncRequest = new SimulationClockSyncRequest(DateTime.UtcNow);
            _client.Peer?.SendRequest<SimulationClockSyncRequest>(simClockSyncRequest, 0, MessageDeliveryMethod.Unreliable);
        }

        private void UpdateTargetSimulationClockOffset(bool immediate)
        {
            _targetClockOffset = _clockSynchronizer.AvgOffset + _simulationTimeOffset;

            TimeSpan delta = _targetClockOffset - Simulation.ClockOffset;

            if (immediate || Math.Abs(delta.TotalMilliseconds) > SmoothClockOffsetThreshold.TotalMilliseconds)
            {
                // Do not wait for smooth correction, apply right away
                Simulation.ClockOffset = _targetClockOffset;
            }
        }

        private void ApplySimulationClockSmoothCorrection()
        {
            if(Simulation.ClockOffset != _targetClockOffset)
            {
                TimeSpan delta = _targetClockOffset - Simulation.ClockOffset;                    
                double stepMs = Math.Max(SmoothClockOffsetCorrectionStepSize.TotalMilliseconds, Math.Abs(delta.TotalMilliseconds));
                Simulation.ClockOffset = TimeSpanExtensions.MoveTowards(Simulation.ClockOffset, _targetClockOffset, TimeSpan.FromMilliseconds(stepMs));
            }
        }
        #endregion

        #region Event Handlers

        void IEventHandler<SimulationClockSyncAckEvent>.OnReceiveEvent(SimulationClockSyncAckEvent evt)
        {
            DateTime timeReceivedResponse = DateTime.UtcNow;

            // Record clock synchronization data
            _clockSynchronizer.RecordSyncResult(evt.TimeSentRequest, evt.TimeReceivedRequest, evt.TimeSentResponse, timeReceivedResponse);

            // Check if this is an initial sync, before simulation has started
            bool initialSync = _clockSyncInitTcs != null;
            
            // Update offset
            UpdateTargetSimulationClockOffset(initialSync);

            // Check if we need to complete initial clock synchronization
            if (initialSync && _clockSynchronizer.NumRoundTripsRecorded >= NumInitialClockSyncEvents)
            {
                var tcs = _clockSyncInitTcs;
                _clockSyncInitTcs = null;
                tcs.SetResult(true);
            }
        }

        void IEventHandler<SimulationInitEvent>.OnReceiveEvent(SimulationInitEvent evt)
        {
            if(!_isJoined)
            {
                return;
            }

            // Set tickrate, to make sure it's the same on both sides
            Simulation.TickRate = evt.SimulationTickRate;

            // Set last known server tick info
            lock (_lastKnownTickSyncRoot)
            {
                _lastKnownServerTickNumber = evt.InitialSnapshot.TickNumber;
                _lastKnownServerTickTime = evt.InitialSnapshot.TickTime;
            }

            // Calculate starting simulation tick number
            uint deltaTicks = (uint)(_simulationTimeOffset.Ticks / Simulation.TimePerTick.Ticks);
            Simulation.CurrentTickNumber = evt.InitialSnapshot.TickNumber + deltaTicks;
            Simulation.CurrentTickTime = evt.InitialSnapshot.TickTime + _clockSynchronizer.AvgOffset + _simulationTimeOffset;

            // Ingest initial snapshot
            IngestTickSnapshot(evt.InitialSnapshot);

            // Start ticking
            RunSimulation();
        }

        void IEventHandler<SimulationTickSnapshotEvent>.OnReceiveEvent(SimulationTickSnapshotEvent evt)
        {
            // Dispatch tick snapshots
            foreach(SimulationTickSnapshot snapshot in evt.TickSnapshots)
            {
                IngestTickSnapshot(snapshot);

                _lastKnownServerTickNumber = snapshot.TickNumber;
                _lastKnownServerTickTime = snapshot.TickTime;
            }

            // Acknowledge tick snapshot
            SimulationTickSnapshotAckRequest ackRequest = new SimulationTickSnapshotAckRequest(evt.TickSnapshots.Last.Value.TickNumber);
            _client.Peer.SendRequest(ackRequest, deliveryMethod: MessageDeliveryMethod.Unreliable);
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            StopSimulation();
        }
        #endregion
    }
}
