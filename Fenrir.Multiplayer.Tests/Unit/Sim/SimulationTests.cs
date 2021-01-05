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
    public class SimulationTests
    {

        #region Simulation.CreateObject
        [TestMethod]
        public void Simulation_CreateObject_AddsSimObject_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_CreateObject_ThrowsSimulationException_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);

            simulation.CreateObject();
        }
        #endregion

        #region Simulation.RemoveObject
        [TestMethod]
        public void Simulation_RemoveObject_RemovesSimObject_UsingObjectRef_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
            
        }

        [TestMethod]
        public void Simulation_RemoveObject_RemovesSimObject_UsingObjectId_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.RemoveObject(simObject.Id);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenServerSim_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            simulation.RemoveObject(new SimulationObject(simulation, 123)); // bad object id
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenClientSim_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            simulation.RemoveObject(123); // bad object id
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);

            simulation.RemoveObject(new SimulationObject(simulation, 123));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);

            simulation.RemoveObject(123);
        }
        #endregion

        #region Simulation.RegisterComponentType
        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }
        #endregion

        #region Simulation.GetComponentTypeHash
        [TestMethod]
        public void Simulation_GetComponentTypeHash_RetrunsDeterministicComponentTypeHash()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();

            var simulation1 = new Simulation(logger, simulationServerMock.Object);
            var simulation2 = new Simulation(logger, simulationServerMock.Object);

            Assert.AreEqual(simulation1.GetComponentTypeHash<TestComponent>(), simulation2.GetComponentTypeHash<TestComponent>());
            Assert.AreEqual(simulation1.GetComponentTypeHash(typeof(TestComponent)), simulation2.GetComponentTypeHash(typeof(TestComponent)));
        }

        #endregion

        #region Simulation.AddPlayer
        [TestMethod]
        public void Simulation_AddPlayer_InvokesSimulationServer()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            simulation.AddPlayer("test_player");

            simulationServerMock.Verify(handler => handler.PlayerAdded(simulation, It.IsAny<SimulationObject>(), "test_player"));
        }

        [TestMethod]
        public void Simulation_AddPlayer_AllowsAddingPlayerComponent()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
            simulation.RegisterComponentType<TestPlayerComponent>();

            SimulationObject playerObj = null;

            simulationServerMock.Setup(handler => handler.PlayerAdded(simulation, It.IsAny<SimulationObject>(), "test_player"))
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
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            simulation.AddPlayer("test_player");

            simulation.RemovePlayer("test_player");

            simulationServerMock.Verify(handler => handler.PlayerRemoved(simulation, It.IsAny<SimulationObject>(), "test_player"));
        }
        #endregion

        #region Simulation.Tick
        [TestMethod]
        public void Simulation_Tick_TicksComponents()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
            simulation.RegisterComponentType<TestTickingComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestTickingComponent comp = simObject.AddComponent<TestTickingComponent>();
            bool componentDidTick = false;
            comp.TickHandler = () => componentDidTick = true;

            simulation.Tick();
            Assert.IsTrue(componentDidTick);
        }

        #endregion

        #region Simulation.Enqueue
        [TestMethod]
        public void Simulation_EnqueueAction_EnqueuesAction()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            bool didInvoke = false;

            simulation.EnqueueAction(() => didInvoke = true);
            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public async Task Simulation_ScheduleAction_SchedulesAction()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            bool didInvoke = false;

            simulation.ScheduleAction(() => didInvoke = true, 50);
            simulation.Tick();

            Assert.IsFalse(didInvoke);

            await Task.Delay(100);

            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }
        #endregion

        #region SimulationObject.AddComponent
        [TestMethod]
        public void SimulationObject_AddComponent_AddsSimulationComponent_UsingDefaultConstructor_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
        public void SimulationObject_AddComponent_AddsSimulationComponent_UsingComponentReference_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingDefaultConstructor_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingComponentReference_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentOfTheSameTypeWasAdded_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            simObject.AddComponent<TestComponent>();
            simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_AddComponent_ThrowsSimulationException_UsingDefaultConstructor_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, 123);
            simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_AddComponent_ThrowsSimulationException_UsingComponentReference_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, 123);
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }
        #endregion

        #region SimulationObject.RemoveComponent
        [TestMethod]
        public void SimulationObject_RemoveComponent_RemovesSimulationComponent_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
        public void SimulationObject_GetOrAddComponent_AddsSimulationComponent_IfNotAdded_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
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
        public void SimulationObject_GetOrAddComponent_ReturnsSimulationComponent_IfWasAddedBefore_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationServerMock = new Mock<ISimulationServer>();
            var simulation = new Simulation(logger, simulationServerMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.CreateObject();
            TestComponent comp1 = simObject.AddComponent<TestComponent>();
            Assert.IsNotNull(comp1);

            // Get or add component
            var comp2 = simObject.GetOrAddComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual(comp1, comp2); // same object
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_GetOrAddComponent_ThrowsSimulationException_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationClientMock = new Mock<ISimulationClient>();
            var simulation = new Simulation(logger, simulationClientMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, 123);
            TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
        }
        #endregion


        // TODO: Add/remove component replication on the other side
        // TODO: RPC
        // TODO: [SyncVar] and 

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

        class TestTickingComponent : SimulationComponent
        {
            public Action TickHandler;

            public override void Tick()
            {
                TickHandler?.Invoke();
            }
        }
        #endregion
    }
}
