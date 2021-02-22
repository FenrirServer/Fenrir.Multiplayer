using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Events;
using Fenrir.Multiplayer.Tests.Fixtures;
using Fenrir.Multiplayer.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Simulation
{
    [TestClass]
    public class SimulationClientTests
    {
        [TestMethod]
        public async Task SimulationClient_SynchronizesSimulationClock()
        {
            var clientPeerMock = new Mock<IClientPeer>();
            clientPeerMock.Setup(peer => peer.SendRequest<RoomJoinRequest, RoomJoinResponse>(It.IsAny<RoomJoinRequest>(), It.IsAny<bool>(), It.IsAny<byte>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new RoomJoinResponse(true)));

            var networkClientMock = new Mock<INetworkClient>();
            networkClientMock.Setup(client => client.State).Returns(ConnectionState.Connected);
            networkClientMock.Setup(client => client.Peer).Returns(clientPeerMock.Object);

            var testLogger = new TestLogger();
            using var simulationClient = new SimulationClient(networkClientMock.Object, testLogger);

            simulationClient.Join("test", "Test").FireAndForget(testLogger);

            TimeSpan fakeServerTimeOffset = TimeSpan.FromSeconds(-100);
            TimeSpan fakeRtt = TimeSpan.Zero;

            DateTime ServerTime() => DateTime.UtcNow + fakeServerTimeOffset;

            // Fake clock sync responses from the server
            var clockSyncAckEventHandler = (IEventHandler<SimulationClockSyncAckEvent>)simulationClient;

            Stopwatch sw = new Stopwatch(); // Used to measure how long test code execution takes to do better measurments

            for (int i=0; i<simulationClient.NumInitialClockSyncEvents; i++)
            {
                var timeSentRequest = DateTime.UtcNow - fakeRtt / 2;
                var timeReceivedRequest = ServerTime();
                var timeSentResponse = ServerTime() + TimeSpan.FromMilliseconds(5);

                sw.Start();
                var clockSyncAckEvent = new SimulationClockSyncAckEvent()
                {
                    TimeSentRequest = timeSentRequest,
                    TimeReceivedRequest = ServerTime(),
                    TimeSentResponse = ServerTime()
                };

                clockSyncAckEventHandler.OnReceiveEvent(clockSyncAckEvent);

                sw.Stop();
                fakeRtt = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds); 
                sw.Reset();
            }

            TimeSpan tickTime = TimeSpan.FromMilliseconds(1000d / simulationClient.Simulation.TickRate);

            // Verify initial clock offset was set (offset + 1 tick)
            TimeSpan expectedOffset = fakeServerTimeOffset + tickTime;
            Assert.AreEqual(expectedOffset.TotalMilliseconds, simulationClient.Simulation.ClockOffset.TotalMilliseconds, 3f);

            // Fake sim init event form the server
            var simulationInitEventHandler = (IEventHandler<SimulationInitEvent>)simulationClient;

            DateTime serverTickTime = ServerTime();
            var simInitEvent = new SimulationInitEvent()
            {
                SimulationTickRate = 60,
                InitialSnapshot = new SimulationTickSnapshot()
                {
                    Commands = new List<ISimulationCommand>(),
                    TickNumber = 100,
                    TickTime = serverTickTime,
                }
            };

            simulationInitEventHandler.OnReceiveEvent(simInitEvent);

            // Verify simulation runs in lockstep with the server
            Assert.AreEqual((uint)101, simulationClient.Simulation.CurrentTickNumber);
            DateTime expectedClientTickTime = serverTickTime + fakeServerTimeOffset + (fakeRtt / 2) + tickTime;
            Assert.AreEqual(expectedClientTickTime.Ticks, simulationClient.Simulation.CurrentTickTime.Ticks, TimeSpan.TicksPerMillisecond);

            // Wait for 5 ticks
            await Task.Delay(5 * tickTime);

            Assert.AreEqual((uint)106, simulationClient.Simulation.CurrentTickNumber);

            expectedClientTickTime = serverTickTime + fakeServerTimeOffset + (fakeRtt / 2) + tickTime + tickTime * 5;
            Assert.AreEqual(expectedClientTickTime.Ticks, simulationClient.Simulation.CurrentTickTime.Ticks, TimeSpan.TicksPerMillisecond);
        }
    }
}
