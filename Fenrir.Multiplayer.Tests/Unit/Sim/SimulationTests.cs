using Fenrir.Multiplayer.Network;
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

        #region Simulation.RegisterComponentType
        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }
        #endregion

        #region Simulation.GetComponentTypeHash
        [TestMethod]
        public void Simulation_GetComponentTypeHash_RetrunsDeterministicComponentTypeHash()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();

            var simulation1 = new Simulation(logger, playerHandlerMock.Object);
            var simulation2 = new Simulation(logger, playerHandlerMock.Object);

            Assert.AreEqual(simulation1.GetComponentTypeHash<TestComponent>(), simulation2.GetComponentTypeHash<TestComponent>());
            Assert.AreEqual(simulation1.GetComponentTypeHash(typeof(TestComponent)), simulation2.GetComponentTypeHash(typeof(TestComponent)));
        }

        #endregion

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

        #region SimulationObject.AddComponent
        [TestMethod]
        public void SimulationObject_AddComponent_AddsSimulationComponent_UsingDefaultConstructor()
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
        public void SimulationObject_AddComponent_AddsSimulationComponent_UsingComponentReference()
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
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingDefaultConstructor()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingComponentReference()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentOfTheSameTypeWasAdded()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            simObject.AddComponent<TestComponent>();
            simObject.AddComponent<TestComponent>();
        }

        #endregion

        #region SimulationObject.RemoveComponent
        [TestMethod]
        public void SimulationObject_RemoveComponent_RemovesSimulationComponent()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

            // Remove component
            simObject.RemoveComponent<TestComponent>();

            // Check if was removed
            Assert.IsFalse(simObject.GetComponents().Contains(testComponent));
        }
        #endregion

        #region SimulationObject.TryGetComponent
        [TestMethod]
        public void SimulationObject_TryGetComponent_ReturnsTrue_WritesSimulationComponent()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

            // Get component
            Assert.IsTrue(simObject.TryGetComponent<TestComponent>(out TestComponent comp));
            Assert.IsNotNull(comp);
        }
        #endregion

        #region SimulationObject.GetComponents
        [TestMethod]
        public void SimulationObject_GetComponents_ReturnsSimulationComponents()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();
            simulation.RegisterComponentType<OtherTestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
            OtherTestComponent testComponent2 = simObject.AddComponent<OtherTestComponent>();

            // Get component
            var components = simObject.GetComponents();
            Assert.IsTrue(components.Contains(testComponent));
            Assert.IsTrue(components.Contains(testComponent2));
        }

        #endregion

        #region SimulationObject.GetOrAddComponent
        [TestMethod]
        public void SimulationObject_GetOrAddComponent_AddsSimulationComponent_IfNotAdded()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
            Assert.IsNotNull(comp1);

            // Get component
            var comp2 = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual(comp1, comp2); // same object
        }

        [TestMethod]
        public void SimulationObject_GetOrAddComponent_ReturnsSimulationComponent_IfWasAddedBefore()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent comp1 = simObject.AddComponent<TestComponent>();
            Assert.IsNotNull(comp1);

            // Get or add component
            var comp2 = simObject.GetOrAddComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual(comp1, comp2); // same object
        }
        #endregion


        // TODO: Add/remove component replication on the other side
        // TODO: RPC
        // TODO: [SyncVar] and rollback

        // TODO: Tick()

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

        class OtherTestComponent : SimulationComponent
        {
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
