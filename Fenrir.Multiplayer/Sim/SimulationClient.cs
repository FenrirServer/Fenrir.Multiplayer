using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Events;
using Fenrir.Multiplayer.Sim.Requests;
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
    {
        private readonly IFenrirLogger _logger;
        private readonly IFenrirClient _client;

        private readonly Simulation _simulation;

        private readonly Stopwatch _simulationTickStopwatch = new Stopwatch();

        private string _roomId = null;

        private bool _isJoined => _roomId != null;

        private bool _isRunningSimulation = false;

        /// <summary>
        /// Total sum of all offsets we have received from clock sync
        /// </summary>
        private long _clockOffsetSumTicks = 0;

        /// <summary>
        /// Total sum of all roundtrip delays we have received from clock sync
        /// </summary>
        private long _clockRoundTripSumTicks = 0;

        /// <summary>
        /// Total sum of all squared deviations for all roundtrip delays received from clock sync
        /// </summary>
        private long _clockRoundTripSumDeviationsSqr = 0;

        /// <summary>
        /// Number of clock offsets we have received from clock sync
        /// </summary>
        private long _numClockSyncsReceived = 0;

        /// <summary>
        /// Last time simulation clock was synced
        /// </summary>
        private DateTime _lastClockSyncTime;
        
        /// <summary>
        /// Task completion source, completes when
        /// simulation synchronization is completed
        /// </summary>
        private TaskCompletionSource<bool> _initTcs = null;

        /// <summary>
        /// Simulation tick rate, how many ticks per second
        /// </summary>
        public int TickRate { get; set; } = 66;

        /// <summary>
        /// Simulation clock sync rate
        /// </summary>
        public float ClockSyncRate { get; set; } = 0.2f; // once every 5 seconds

        /// <summary>
        /// Number of initial clock synchronization request
        /// </summary>
        public int NumInitialClockSyncRequests { get; set; } = 5;

        /// <summary>
        /// Delay between initial clock sync requests
        /// </summary>
        public double InitialClockSyncDelayMs { get; set; } = 5;

        public SimulationClient(IFenrirLogger logger, IFenrirClient client)
        {
            _logger = logger;
            _client = client;
            _client.Disconnected += OnDisconnected;

            _simulation = new Simulation(logger) { IsAuthority = false };
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
            await SyncSimulationClockInitially();

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
                // Tick simulation
                await TickSimulation();

                // Check if we need to sync clock again
                double delayBetweenClockSyncs = 1000 / ClockSyncRate;
                DateTime nextClockSyncTime = _lastClockSyncTime + TimeSpan.FromMilliseconds(delayBetweenClockSyncs);
                if(DateTime.UtcNow > nextClockSyncTime)
                {
                    _ = SendSyncClockRequest();
                }
            }
        }

        private async Task TickSimulation()
        {
            _simulationTickStopwatch.Start();

            double delayBetweenTicksMs = 1000d / TickRate;

            try
            {
                _simulation.Tick();
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
                    _simulation.IngestCommand(command);
                }
            }
        }

        #region Clock Sync
        private void SetSimulationClockOffset(TimeSpan offset)
        {
            _simulation.SetClockOffset(offset);
        }

        private async Task SyncSimulationClockInitially()
        {
            await Task.WhenAll(SyncSimulationClockTasks());

            long offsetAvg = _clockOffsetSumTicks / _numClockSyncsReceived;

            _simulation.SetClockOffset(TimeSpan.FromTicks(offsetAvg));
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

            DateTime timeSentRequest = DateTime.UtcNow;

            var simClockSyncResponse = await _client.Peer.SendRequest<SimulationClockSyncRequest, SimulationClockSyncResponse>(simClockSyncRequest);

            DateTime timeReceivedResponse = DateTime.UtcNow;


            // Check if this response is an outlier, by comparing it's roundtrip time against the standard deviation
            TimeSpan roundTripTime = timeReceivedResponse - timeSentRequest;

            // Calculate standard deviation for previously received roundtrip times
            // https://en.wikipedia.org/wiki/Standard_deviation
            long roundTripTotalVariance = _clockRoundTripSumDeviationsSqr / _numClockSyncsReceived;
            long roundTripTotalStandardDeviation = (long)Math.Sqrt(roundTripTotalVariance);

            // Calculate deviation of this sync round trip time from the average
            long roundTripTotalTicksAvg = _clockRoundTripSumTicks / _numClockSyncsReceived;
            long roundTripDeviation = roundTripTime.Ticks - roundTripTotalTicksAvg;

            // Check if current round trip deviation is too big, if so, filter out this response.
            // For this case too big means smaller than half the standard deviation or bigger than twice the standard deviation
            if(roundTripDeviation < roundTripTotalStandardDeviation / 2 ||  roundTripDeviation > roundTripTotalStandardDeviation * 2)
            {
                return; // Outlier, ignore
            }

            // Calculate time offset and record this sync clock result

            // https://en.wikipedia.org/wiki/Network_Time_Protocol
            long offsetDelayTicks = ((simClockSyncResponse.ServerTime - timeSentRequest) + (simClockSyncResponse.ServerTime - timeReceivedResponse)).Ticks / 2;
            _clockOffsetSumTicks += offsetDelayTicks;
            _clockRoundTripSumTicks += roundTripTime.Ticks;
            _clockRoundTripSumDeviationsSqr += (roundTripDeviation * roundTripDeviation);
            _numClockSyncsReceived++;
        }
        #endregion

        #region Event Handlers
        async void IEventHandler<SimulationInitEvent>.OnReceiveEvent(SimulationInitEvent evt)
        {
            // Apply snapshot
            ApplyTickSnapshot(evt.SimulationSnapshot);

            // Start ticking
            RunSimulationTickLoop();

            // Wait until we buffer simulation ticks
            await Task.Delay(TimeSpan.FromMilliseconds(_simulation.IncomingCommandDelayMs));

            // If we are still joined, start running. 
            // We should already have enough commands in the buffer to start running and interpolate.
            if (_isJoined)
            {
                _initTcs?.SetResult(true);
                _isRunningSimulation = true;
            }
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
