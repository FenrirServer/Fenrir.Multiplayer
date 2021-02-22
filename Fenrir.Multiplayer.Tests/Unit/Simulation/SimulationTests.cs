using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Simulation
{
    [TestClass]
    public class SimulationTests
    {
        #region Simulation.SpawnObject
        [TestMethod]
        public void Simulation_SpawnObject_ThrowsSimulationException_WhenNoAuthority()
        {
            // Client simulation does not allow creating objects

            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };

            simulation.EnqueueAction(() => 
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    simulation.SpawnObject();
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void Simulation_SpawnObject_AddsSimulationObject_AddsObject_SendsSimulationCommand_WhenHasAuthority()
        {
            // Client simulation does not allow creating objects

            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            SpawnObjectSimulationCommand command = null;
            simulation.CommandCreated += cmd => command = (SpawnObjectSimulationCommand)cmd;

            SimulationObject simObject = null;

            simulation.EnqueueAction(() => simObject = simulation.SpawnObject());
            simulation.Tick(); // Executes the action and sends the outgoing command

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }
        #endregion

        #region Simulation.GetObject
        [TestMethod]
        public void Simulation_GetObject_ReturnsObject()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
            SimulationObject simObject = null;

            simulation.EnqueueAction(() => simObject = simulation.SpawnObject());
            simulation.Tick(); // Executes the action and sends the outgoing command

            SimulationObject obj2 = null;
            simulation.EnqueueAction(() => obj2 = simulation.GetObject(simObject.Id));
            simulation.Tick();

            Assert.IsNotNull(obj2);
        }
        #endregion

        #region Simulation.TryGetObject
        [TestMethod]
        public void Simulation_TryGetObject_ReturnsObject()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
            SimulationObject simObject = null;

            simulation.EnqueueAction(() => simObject = simulation.SpawnObject());
            simulation.Tick(); // Executes the action and sends the outgoing command

            bool result = false;
            SimulationObject obj2 = null;
            simulation.EnqueueAction(() => result = simulation.TryGetObject(simObject.Id, out obj2));
            simulation.Tick();

            Assert.IsNotNull(obj2);
        }
        #endregion

        #region Simulation.GetObjects
        [TestMethod]
        public void Simulation_GetObjects_ReturnsObjectEnumerable()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
            SimulationObject simObject = null;

            simulation.EnqueueAction(() => simObject = simulation.SpawnObject());
            simulation.Tick(); // Executes the action and sends the outgoing command

            IEnumerable<SimulationObject> simObjects = null;
            simulation.EnqueueAction(() => simObjects = simulation.GetObjects());
            simulation.Tick();

            Assert.AreEqual(1, simObjects.Count());
            Assert.IsTrue(simObjects.Any(obj => obj.Id == simObject.Id));
        }
        #endregion

        #region Simulation.RemoveObject
        [TestMethod]
        public void Simulation_DestroyObject_ThrowsSimulationException_UsingObjectRef_WhenNoAuthority()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    simulation.DestroyObject(new SimulationObject(simulation, logger, 123));
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void Simulation_DestroyObject_ThrowsSimulationException_UsingObjectId_WhenNoAuthority()
        {
            // Client simulation does not allow removing objects

            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    simulation.DestroyObject(123);
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void ServerSimulation_DestroyObject_RemovesSimObject_SendsSimulationCommand_UsingObjectRef_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            DestroyObjectSimulationCommand command = null;
            simulation.CommandCreated += cmd =>
            {
                if (cmd is DestroyObjectSimulationCommand)
                {
                    command = (DestroyObjectSimulationCommand)cmd;
                }
            };

            SimulationObject simObject = null;

            simulation.EnqueueAction(() => simObject = simulation.SpawnObject());
            simulation.Tick(); // Executes the action and sends the outgoing command

            Assert.IsNotNull(simObject);

            Assert.IsTrue(simulation.HasObject(simObject.Id));

            simulation.EnqueueAction(() => simulation.DestroyObject(simObject));
            simulation.Tick();

            Assert.IsFalse(simulation.HasObject(simObject.Id));

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }

        [TestMethod]
        public void ServerSimulation_DestroyObject_RemovesSimObject_SendsSimulationCommand_UsingObjectId_WhenHasAuthority()
        {

            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            DestroyObjectSimulationCommand command = null;
            simulation.CommandCreated += cmd =>
            {
                if (cmd is DestroyObjectSimulationCommand)
                {
                    command = (DestroyObjectSimulationCommand)cmd;
                }
            };

            SimulationObject simObject = null;
            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                Assert.IsNotNull(simObject);
            });

            simulation.Tick();

            Assert.IsTrue(simulation.GetObjects().Contains(simObject));

            simulation.EnqueueAction(() => simulation.DestroyObject(simObject.Id));
            simulation.Tick(); // Executes the action and sends the outgoing command

            Assert.IsFalse(simulation.GetObjects().Contains(simObject));

            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
        }

        [TestMethod]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectRef_WhenNoObjectFound()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    simulation.DestroyObject(new SimulationObject(simulation, logger, 123)); // bad object id
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void ServerSimulation_RemoveObject_ThrowsSimulationException_UsingObjectId_WhenNoObjectFound()
        {
            var logger = new TestLogger(); 
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    simulation.DestroyObject(123); // bad object id
                });
            });
            simulation.Tick();
        }
        #endregion

        #region Simulation.RegisterComponentType
        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            simulation.RegisterComponentType<TestComponent>();

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RegisterComponentType_ThrowsSimulationException_WhenComponentRegistered()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            simulation.RegisterComponentType<TestComponent>();
            simulation.RegisterComponentType<TestComponent>();
        }


        [TestMethod]
        public void Simulation_RegisterComponentType_RegistersComponent_WithComponentFactory()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            simulation.RegisterComponentType<TestComponent>(() => new TestComponent());

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void Simulation_RegisterComponentType_ThrowsSimulationException_WhenComponentRegistered_WithComponentFactory()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            simulation.RegisterComponentType<TestComponent>(() => new TestComponent());
            simulation.RegisterComponentType<TestComponent>(() => new TestComponent());

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void Simulation_RegisterComponentType_ThrowsArgumentNullException_WhenFactoryMethodIsNull()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            simulation.RegisterComponentType<TestComponent>(null);

            Assert.IsTrue(simulation.ComponentRegistered<TestComponent>());
        }
        #endregion

        #region Simulation.ProcessTickSnapshot

        #region Simulation.ProcessTickSnapshot - SpawnObjectSimulationCommand
        [TestMethod]
        public void Simulation_ProcessTickSnapshot_SpawnObjectSimulationCommand_SpawnsSimulationObject()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };

            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot(1, DateTime.UtcNow);
            tickSnapshot.Commands.Add(spawnObjectCommand);

            simulation.IngestTickSnapshot(tickSnapshot);

            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));
        }
        #endregion

        #region Simulation.IngestCommand(DestroyObjectSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_DestroyObjectSimulationCommand_DestroysSimulationObject()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };

            // Create object
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot(1, DateTime.UtcNow);
            tickSnapshot.Commands.Add(spawnObjectCommand);

            simulation.IngestTickSnapshot(tickSnapshot);

            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Destroy object
            var destroyObjectCommand = new DestroyObjectSimulationCommand(123);
            tickSnapshot = new SimulationTickSnapshot(2, DateTime.UtcNow);
            tickSnapshot.Commands.Add(destroyObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

            simulation.RegisterComponentType<TestTickingComponent>();

            bool componentDidTick = false;

            simulation.EnqueueAction(() =>
            {
                SimulationObject simObject = simulation.SpawnObject();
                TestTickingComponent comp = simObject.AddComponent<TestTickingComponent>();
                comp.TickHandler = () => componentDidTick = true;
            });

            simulation.Tick();
            Assert.IsTrue(componentDidTick);
        }

        #endregion

        #region Simulation.EnqueueAction
        [TestMethod]
        public void Simulation_EnqueueAction_EnqueuesAction()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            bool didInvoke = false;

            simulation.EnqueueAction(() => didInvoke = true);
            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public async Task Simulation_ScheduleAction_SchedulesAction()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            bool didInvoke = false;

            simulation.ScheduleAction(() => didInvoke = true, 50);
            simulation.Tick();

            Assert.IsFalse(didInvoke);

            await Task.Delay(100);

            simulation.Tick();

            Assert.IsTrue(didInvoke);
        }
        #endregion

        #region Simulation.EnqueueLateAction
        [TestMethod]
        public void Simulation_EnqueueLateAction_EnqueuesLateAction()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            bool didInvoke = false;
            bool didInvokeLate = false;

            simulation.EnqueueLateAction(() => didInvokeLate = true);
            simulation.EnqueueAction(() =>
            {
                Assert.IsFalse(didInvokeLate, "should not execute Late action before a regular one");
                didInvoke = true;
            });
            simulation.Tick();

            Assert.IsTrue(didInvoke);
            Assert.IsTrue(didInvokeLate);
        }

        [TestMethod]
        public async Task Simulation_ScheduleLateAction_SchedulesLateAction()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger);

            bool didInvoke = false;
            bool didInvokeLate = false;

            simulation.ScheduleLateAction(() => didInvokeLate = true, 50);
            simulation.ScheduleAction(() =>
            {
                Assert.IsFalse(didInvokeLate, "should not execute Late action before a regular one");
                didInvoke = true;
            }
            , 50);
            simulation.Tick();

            Assert.IsFalse(didInvoke);
            Assert.IsFalse(didInvokeLate);

            await Task.Delay(100);

            simulation.Tick();

            Assert.IsTrue(didInvoke);
            Assert.IsTrue(didInvokeLate);
        }
        #endregion

        #region Simulation.IngestCommand
        #region Simulation.IngestCommand(AddComponentSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_AddComponentSimulationCommand_AddsSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Create object
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot(1, DateTime.UtcNow);
            tickSnapshot.Commands.Add(spawnObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Add component
            var addComponentCommand = new AddComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot(2, DateTime.UtcNow);
            tickSnapshot.Commands.Add(addComponentCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            // Get component
            TestComponent comp = null;
            simulation.EnqueueAction(() =>
            {
                var simObject = simulation.GetObject(123);
                Assert.IsNotNull(simObject);
                comp = simObject.GetComponent<TestComponent>();
            });
            simulation.Tick();

            Assert.IsNotNull(comp);
        }

        #endregion


        #region Simulation.IngestCommand(RemoveComponentSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_RemoveComponentSimulationCommand_RemovesSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            TimeSpan tickTime = TimeSpan.FromMilliseconds(1000d / simulation.TickRate);

            // Create object
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot(1, DateTime.UtcNow);
            tickSnapshot.Commands.Add(spawnObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Add component
            var addComponentCommand = new AddComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot(2, DateTime.UtcNow + tickTime);
            tickSnapshot.Commands.Add(addComponentCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            // Get component
            TestComponent comp = null;
            simulation.EnqueueAction(() =>
            {
                var simObject = simulation.GetObject(123);
                Assert.IsNotNull(simObject);
                comp = simObject.GetComponent<TestComponent>();
            });
            simulation.Tick();

            Assert.IsNotNull(comp);

            // Remove component
            var removeComponentCommand = new RemoveComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot(3, DateTime.UtcNow + tickTime*2);
            tickSnapshot.Commands.Add(removeComponentCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            simulation.EnqueueAction(() =>
            {
                var simObject = simulation.GetObject(123);
                Assert.IsNotNull(simObject);
                comp = simObject.GetComponent<TestComponent>();
            });
            simulation.Tick();

            Assert.IsNull(comp);
        }

        #endregion

        #endregion
    }
}
