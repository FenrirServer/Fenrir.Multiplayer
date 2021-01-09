using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationTests
    {
        #region Simulation.SpawnObject
        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_SpawnObject_ThrowsSimulationException()
        {
            // Client simulation does not allow creating objects

            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            simulation.SpawnObject();
        }
        #endregion

        #region Simulation.RemoveObject
        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenClientSim()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            simulation.RemoveObject(new SimulationObject(simulation, 123));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenClientSim()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            simulation.RemoveObject(123);
        }

        #endregion

        #region Simulation.RegisterComponentType
        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            simulation.RegisterComponentType<TestComponent>();

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }
        #endregion

        #region Simulation.GetComponentTypeHash
        [TestMethod]
        public void Simulation_GetComponentTypeHash_RetrunsDeterministicComponentTypeHash()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation1 = new Simulation(logger, simulationViewMock.Object);
            var simulation2 = new Simulation(logger, simulationViewMock.Object);

            Assert.AreEqual(simulation1.GetComponentTypeHash<TestComponent>(), simulation2.GetComponentTypeHash<TestComponent>());
            Assert.AreEqual(simulation1.GetComponentTypeHash(typeof(TestComponent)), simulation2.GetComponentTypeHash(typeof(TestComponent)));
        }
        #endregion

        #region Simulation.Tick
        [TestMethod]
        public void Simulation_Tick_TicksComponents()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            simulation.RegisterComponentType<TestTickingComponent>();

            SimulationObject simObject = simulation.SpawnObject();
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            bool didInvoke = false;

            simulation.EnqueueAction(() => didInvoke = true);
            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public async Task Simulation_ScheduleAction_SchedulesAction()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);

            bool didInvoke = false;

            simulation.ScheduleAction(() => didInvoke = true, 50);
            simulation.Tick();

            Assert.IsFalse(didInvoke);

            await Task.Delay(100);

            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }
        #endregion
    }
}
