using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationTests
    {
        #region Simulation.SpawnObject
        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_SpawnObject_ThrowsSimulationException_WhenNoAuthority()
        {
            // Client simulation does not allow creating objects

            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = false }; 

            simulation.SpawnObject();
        }

        [TestMethod]
        public void Simulation_SpawnObject_AddsSimulationObject_AddsObject_SendsSimulationCommand_WhenHasAuthority()
        {
            // Client simulation does not allow creating objects

            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();

            SpawnObjectSimulationCommand command = null;
            simulationListenerMock.Setup(listener => listener.OnSendCommand(It.IsAny<ISimulationCommand>()))
                .Callback<ISimulationCommand>(cmd => command = (SpawnObjectSimulationCommand)cmd);

            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = true };
            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }
        #endregion

        #region Simulation.RemoveObject
        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_DestroyObject_ThrowsSimulationException_UsingObjectRef_WhenNoAuthority()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = false };

            simulation.DestroyObject(new SimulationObject(simulation, logger, 123));
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_DestroyObject_ThrowsSimulationException_UsingObjectId_WhenNoAuthority()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = false };

            simulation.DestroyObject(123);
        }

        [TestMethod]
        public void ServerSimulation_DestroyObject_RemovesSimObject_SendsSimulationCommand_UsingObjectRef_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();

            DestroyObjectSimulationCommand command = null;
            simulationListenerMock.Setup(listener => listener.OnSendCommand(It.IsAny<ISimulationCommand>()))
                .Callback<ISimulationCommand>(cmd => command = (DestroyObjectSimulationCommand)cmd);


            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = true };

            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.HasObject(simObject.Id));

            simulation.DestroyObject(simObject);

            Assert.IsFalse(simulation.HasObject(simObject.Id));

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }

        [TestMethod]
        public void ServerSimulation_DestroyObject_RemovesSimObject_SendsSimulationCommand_UsingObjectId_WhenHasAuthority()
        {

            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();

            DestroyObjectSimulationCommand command = null;
            simulationListenerMock.Setup(listener => listener.OnSendCommand(It.IsAny<ISimulationCommand>()))
                .Callback<ISimulationCommand>(cmd => command = (DestroyObjectSimulationCommand)cmd);


            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = true };

            SimulationObject simObject = simulation.SpawnObject();

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.DestroyObject(simObject.Id);

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger);

            simulation.DestroyObject(new SimulationObject(simulation, logger, 123)); // bad object id
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger);

            simulation.DestroyObject(123); // bad object id
        }
        #endregion

        #region Simulation.RegisterComponentType
        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger);

            simulation.RegisterComponentType<TestComponent>();

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }
        #endregion

        #region Simulation.IngestCommand

        #region Simulation.IngestCommand(SpawnObjectSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_SpawnObjectSimulationCommand_SpawnsSimulationObject()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = false };

            var spawnObjectCommand = new SpawnObjectSimulationCommand(DateTime.UtcNow, 123);
            simulation.IngestCommand(spawnObjectCommand);

            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));
        }
        #endregion

        #region Simulation.IngestCommand(DestroyObjectSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_DestroyObjectSimulationCommand_DestroysSimulationObject()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = false };

            // Create object
            var spawnObjectCommand = new SpawnObjectSimulationCommand(DateTime.UtcNow, 123);
            simulation.IngestCommand(spawnObjectCommand);

            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Destroy object
            var destroyObjectCommand = new DestroyObjectSimulationCommand(DateTime.UtcNow, 123);
            simulation.IngestCommand(destroyObjectCommand);

            simulation.Tick();

            Assert.IsFalse(simulation.HasObject(spawnObjectCommand.ObjectId));
        }
        #endregion

        #endregion

        #region Simulation.Tick
        [TestMethod]
        public void Simulation_Tick_TicksComponent()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger) { IsAuthority = true };

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
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger);

            bool didInvoke = false;

            simulation.EnqueueAction(() => didInvoke = true);
            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public async Task Simulation_ScheduleAction_SchedulesAction()
        {
            var logger = new TestLogger();
            var simulationListenerMock = new Mock<ISimulationListener>();
            var simulation = new Simulation(simulationListenerMock.Object, logger);

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
