using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Components;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System;
using System.Linq;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationTests
    {

        #region Simulation.CreateObject
        [TestMethod]
        public void Simulation_CreateObject_AddSimObject()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));
        }
        #endregion

        #region Simulation.RemoveObject
        [TestMethod]
        public void Simulation_RemoveObject_RemovesSimObject_UsingObjectRef()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
            
        }

        [TestMethod]
        public void Simulation_RemoveObject_RemovesSimObject_UsingObjectId()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject.Id);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
        }
        #endregion

        #region Simulation.AddComponent
        [TestMethod]
        public void Simulation_AddComponent_AddsSimulationComponent_UsingDefaultConstructor()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

            Assert.AreEqual("test", testComponent.Value);

            // Get component
            var comp2 = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual("test", comp2.Value);
        }

        [TestMethod]
        public void Simulation_AddComponent_AddsSimulationComponent_UsingComponentReference()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);

            Assert.AreEqual("test2", comp.Value);

            // Get component
            var comp2 = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual("test2", comp2.Value);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Simulation_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingDefaultConstructor()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void Simulation_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingComponentReference()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }
        #endregion

        // TODO: Add/remove component replication on the other side
        // TODO: RPC
        // TODO: [SyncVar] and rollback

        // TODO: Remove component
        // TODO: Tick()
        // Get component / get components
        // Fail to add component of the same type
        // Null arg checks


        #region Simulation.AddPlayer
        [TestMethod]
        public void Simulation_AddPlayer_InvokesPlayerHandler()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            simulation.AddPlayer("test_player");

            playerHandlerMock.Verify(handler => handler.PlayerAdded(simulation, It.IsAny<SimulationObject>(), "test_player"));
        }

        [TestMethod]
        public void Simulation_AddPlayer_AllowsAddingPlayerComponent()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestPlayerComponent>();

            SimulationObject playerObj = null;

            playerHandlerMock.Setup(handler => handler.PlayerAdded(simulation, It.IsAny<SimulationObject>(), "test_player"))
                .Callback<Simulation, SimulationObject, string>((sim, playerObject, playerId) =>
                {
                    playerObject.AddComponent<TestPlayerComponent>(new TestPlayerComponent(playerId, "Test Player"));
                    playerObj = playerObject;
                });

            simulation.AddPlayer("test_player");

            Assert.IsNotNull(playerObj);

            TestPlayerComponent playerComp = playerObj.GetComponent<TestPlayerComponent>();

            Assert.IsNotNull(playerComp);
            Assert.AreEqual("Test Player", playerComp.PlayerName);
        }

        #endregion

        #region Simulation.RemovePlayer
        [TestMethod]
        public void Simulation_RemovePlayer_InvokesPlayerHandler()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            simulation.AddPlayer("test_player");

            simulation.RemovePlayer("test_player");

            playerHandlerMock.Verify(handler => handler.PlayerRemoved(simulation, It.IsAny<SimulationObject>(), "test_player"));
        }
        #endregion


        #region Test Fixtures
        class TestComponent : SimulationComponent
        {
            public string Value;

            public TestComponent()
            {
                Value = "test";
            }

            public TestComponent(string value)
            {
                Value = value;
            }
        }

        class TestPlayerComponent : PlayerComponent
        {
            public string PlayerName;

            public TestPlayerComponent(string peerId, string playerName) : base(peerId)
            {
                PlayerName = playerName;
            }
        }

        #endregion
    }
}
