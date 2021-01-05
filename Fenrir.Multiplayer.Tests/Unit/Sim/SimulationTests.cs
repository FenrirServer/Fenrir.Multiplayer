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


        // TODO: Add/remove component replication on the other side
        // TODO: RPC
        // TODO: [SyncVar] and 

    }
}
