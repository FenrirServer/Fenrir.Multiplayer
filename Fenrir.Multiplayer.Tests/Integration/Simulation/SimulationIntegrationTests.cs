using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Components;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Integration.Simulation
{
    [TestClass]
    public class SimulationIntegrationTests
    {
        [TestMethod]
        public async Task Simulation_Integration_ConnectAndJoin()
        {
            using var logger = new TestLogger();
            
            // Create server
            using var networkServer = new NetworkServer(logger);
            networkServer.AddLiteNetProtocol();
            networkServer.AddInfoService();

            // Add server simulation
            var simulationRoomManager = new SimulationRoomManager<SimulationRoom>((peer, roomId, token) => new SimulationRoom(logger, roomId), logger, networkServer);
            
            // Start server
            await networkServer.Start();
            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            // Create client
            using var networkClient = new NetworkClient(logger);
            networkClient.AddLiteNetProtocol();
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:8080");

            // Create simulation client
            var simulationClient = new SimulationClient(networkClient, logger);

            // Connect
            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            // Join simulation
            await simulationClient.Join("testRoom", "testToken");

            // Make sure there is one simulation entity - our player
            Assert.AreEqual(1, simulationClient.Simulation.GetObjects().Count());

            // Get player component
            Assert.IsNotNull(simulationClient.Simulation.GetObjects().First().GetComponent<PlayerComponent>());
        }


        [TestMethod]
        public async Task Simulation_Integration_SpawnObject_AddComponent_DestroyObject_RemoveComponent()
        {
            using var logger = new TestLogger();

            // Create server
            using var networkServer = new NetworkServer(logger);
            networkServer.AddLiteNetProtocol();
            networkServer.AddInfoService();

            // Add server simulation
            var simulationRoomManager = new SimulationRoomManager<SimulationRoom>((peer, roomId, token) => new SimulationRoom(logger, roomId), logger, networkServer);

            // Start server
            await networkServer.Start();
            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            // Create client
            using var networkClient = new NetworkClient(logger);
            networkClient.AddLiteNetProtocol();
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:8080");

            // Create simulation client
            var simulationClient = new SimulationClient(networkClient, logger);

            // Connect
            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            // Join simulation
            await simulationClient.Join("testRoom", "testToken");

            // Get server room
            SimulationRoom room = simulationRoomManager.GetRooms().First();

            // Get simulations
            NetworkSimulation serverSimulation = room.Simulation;
            NetworkSimulation clientSimulation = simulationClient.Simulation;

            serverSimulation.RegisterComponentType<TestComponent>();
            clientSimulation.RegisterComponentType<TestComponent>();

            // --------------------------------------------
            // Spawn server object
            SimulationObject testServerObject = null;
            TaskCompletionSource<bool> serverTickTcs = new TaskCompletionSource<bool>();
            serverSimulation.EnqueueAction(() =>
            {
                testServerObject = serverSimulation.SpawnObject();
                serverTickTcs.SetResult(true);
            });
            await serverTickTcs.Task;

            // Verify object spawned
            Assert.AreEqual(2, serverSimulation.GetObjects().Count());

            // On the client, this object will be spawned N ticks later, since client is running everything behind
            Assert.AreEqual(1, clientSimulation.GetObjects().Count());

            // Wait for client to dispatch incoming tick
            await Task.Delay(clientSimulation.IncomingCommandDelayMs);

            // Wait until next client tick
            await clientSimulation.WaitForNextTick();

            // Verify object has spawned on the client
            Assert.AreEqual(2, clientSimulation.GetObjects().Count());

            // Get client object
            SimulationObject testClientObject = clientSimulation.GetObjects().Skip(1).First();

            // Verify same object id...
            Assert.AreEqual(testServerObject.Id, testClientObject.Id);

            // --------------------------------------------
            // Add component on the server
            TestComponent testServerComponent = null;
            serverTickTcs = new TaskCompletionSource<bool>();
            serverSimulation.EnqueueAction(() => 
            {
                testServerComponent = testServerObject.AddComponent<TestComponent>();
                serverTickTcs.SetResult(true);
            });
            await serverTickTcs.Task;

            // Verify no component on the client yet
            Assert.IsNull(testClientObject.GetComponent<TestComponent>());

            // Wait for client to dispatch incoming tick
            await Task.Delay(clientSimulation.IncomingCommandDelayMs);
            await clientSimulation.WaitForNextTick();

            // Verify client has this component
            Assert.IsNotNull(testClientObject.GetComponent<TestComponent>());

            // --------------------------------------------
            // Remove component on the server
            serverTickTcs = new TaskCompletionSource<bool>();
            serverSimulation.EnqueueAction(() =>
            {
                testServerObject.RemoveComponent<TestComponent>();
                serverTickTcs.SetResult(true);
            });
            await serverTickTcs.Task;

            // Verify component was removed on the server
            Assert.IsNull(testServerObject.GetComponent<TestComponent>());

            // Verify component still exists on the client
            Assert.IsNotNull(testClientObject.GetComponent<TestComponent>());

            // Wait for client to dispatch incoming tick
            await Task.Delay(clientSimulation.IncomingCommandDelayMs);
            await clientSimulation.WaitForNextTick();

            // Verify client no longer has this component
            Assert.IsNull(testClientObject.GetComponent<TestComponent>());

            // --------------------------------------------
            // Destroy server object
            serverTickTcs = new TaskCompletionSource<bool>();
            serverSimulation.EnqueueAction(() =>
            {
                serverSimulation.DestroyObject(testServerObject.Id);
                serverTickTcs.SetResult(true);
            });
            await serverTickTcs.Task;


            // Verify object was destroyed on the server
            Assert.AreEqual(1, serverSimulation.GetObjects().Count());

            // Verify object still exists on the client.
            Assert.AreEqual(2, clientSimulation.GetObjects().Count());

            // Wait for client to dispatch incoming tick
            await Task.Delay(clientSimulation.IncomingCommandDelayMs);

            // Wait until next client tick
            await clientSimulation.WaitForNextTick();

            // Verify object is not in fact destroyed on the client
            Assert.AreEqual(1, clientSimulation.GetObjects().Count());
        }
    }
}
