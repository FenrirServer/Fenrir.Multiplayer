using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Integration.Sim
{
    [TestClass]
    public class SimulationIntegrationTests
    {
        [TestMethod]
        public async Task Simulation_Integration_ConnectAndJoin()
        {
            using var logger = new TestLogger();
            
            // Create server
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            // Add server simulation
            var simulationRoomManager = new SimulationRoomManager(logger, fenrirServer);
            
            // Start server
            await fenrirServer.Start();
            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            // Create client
            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            // Create simulation client
            var simulationClient = new SimulationClient(fenrirClient, logger);

            // Connect
            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            // Join simulation
            await simulationClient.Join("testRoom", "testToken");

            // Make sure there is one simulation entity - our player
            Assert.AreEqual(1, simulationClient.Simulation.GetObjects().Count());

        }
    }
}
