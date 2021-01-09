using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Components;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class ServerSimulationTests
    {

        #region ServerSimulation.CreateObject
        [TestMethod]
        public void ServerSimulation_CreateObject_AddsSimObject()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));
        }
        #endregion

        #region ServerSimulation.RemoveObject
        [TestMethod]
        public void ServerSimulation_RemoveObject_RemovesSimObject_UsingObjectRef()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
            
        }

        [TestMethod]
        public void ServerSimulation_RemoveObject_RemovesSimObject_UsingObjectId()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject.Id);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            simulation.RemoveObject(new SimulationObject(simulation, 123)); // bad object id
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenClientSim_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            simulation.RemoveObject(123); // bad object id
        }
        #endregion

        #region ServerSimulation.AddPlayer
        [TestMethod]
        public void ServerSimulation_AddPlayer_InvokesSimulationServer()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            var serverPeerMock = new Mock<IServerPeer>();

            simulation.AddPlayer(serverPeerMock.Object, "token");

            simulationServerViewMock.Verify(view => view.PlayerJoined(simulation, It.IsAny<SimulationObject>(), serverPeerMock.Object, "token"));
        }

        [TestMethod]
        public void ServerSimulation_AddPlayer_AllowsAddingPlayerComponent()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            simulation.RegisterComponentType<TestPlayerComponent>();

            var serverPeerMock = new Mock<IServerPeer>();
            serverPeerMock.Setup(peer => peer.Id).Returns("test_player");

            SimulationObject playerObj = null;

            simulationServerViewMock.Setup(view => view.PlayerJoined(simulation, It.IsAny<SimulationObject>(), serverPeerMock.Object, "token"))
                .Callback<Simulation, SimulationObject, IServerPeer, string>((sim, playerObject, serverPeer, token) =>
                {
                    Assert.AreEqual("token", token);
                    Assert.AreEqual("test_player", serverPeer.Id);

                    playerObject.AddComponent<TestPlayerComponent>(new TestPlayerComponent(serverPeer.Id, "Test Player"));
                    playerObj = playerObject;
                });

            simulation.AddPlayer(serverPeerMock.Object, "token");

            Assert.IsNotNull(playerObj);

            TestPlayerComponent playerComp = playerObj.GetComponent<TestPlayerComponent>();

            Assert.IsNotNull(playerComp);
            Assert.AreEqual("Test Player", playerComp.PlayerName);
        }

        #endregion

        #region ServerSimulation.RemovePlayer
        [TestMethod]
        public void ServerSimulation_RemovePlayer_InvokesPlayerHandler()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
            
            var serverPeerMock = new Mock<IServerPeer>();

            simulation.AddPlayer(serverPeerMock.Object, "token");

            simulation.RemovePlayer(serverPeerMock.Object);

            simulationServerViewMock.Verify(view => view.PlayerLeft(simulation, It.IsAny<SimulationObject>(), serverPeerMock.Object));
        }
        #endregion

        // TODO: Add/remove component replication on the other side
        // TODO: RPC
        // TODO: [SyncVar] and 

    }
}
