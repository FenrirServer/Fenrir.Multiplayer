using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Dto;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationObjectTests
    {
        #region SimulationObject.AddComponent

        [TestMethod]
        public void SimulationObject_AddComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    SimulationObject simObject = new SimulationObject(simulation, logger, 123);
                    simObject.AddComponent<TestComponent>();
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void SimulationObject_AddComponent_AddsSimulationComponent_SendsAddComponentCommand_UsingDefaultFactory_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            AddComponentSimulationCommand command = null;
            simulation.CommandCreated += cmd =>
            {
                if (cmd is AddComponentSimulationCommand)
                {
                    command = (AddComponentSimulationCommand)cmd;
                }
            };

            SimulationObject simObject = null;
            TestComponent testComponent = null;
            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                testComponent = simObject.AddComponent<TestComponent>();
            });
            simulation.Tick(); // Executes action and sends the command

            Assert.AreEqual("test", testComponent.Value);

            // Get component
            TestComponent component = null;
            simulation.EnqueueAction(() => component = simObject.GetComponent<TestComponent>());
            simulation.Tick();

            Assert.IsNotNull(component);
            Assert.AreEqual("test", component.Value);

            // Verify command was sent
            Assert.IsNotNull(command);
            Assert.AreEqual(simulation.GetComponentTypeHash<TestComponent>(), command.ComponentTypeHash);
            Assert.AreEqual(component.Object.Id, command.ObjectId);
        }

        [TestMethod]
        public void SimulationObject_AddComponent_ThrowsSimulationException_IfComponentNotRegistered_UsingDefaultConstructor_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    SimulationObject simObject = simulation.SpawnObject();
                    TestComponent testComponent = simObject.AddComponent<TestComponent>();
                });
            });
            simulation.Tick();
        }

        [TestMethod]
        public void SimulationObject_AddComponent_ThrowsSimulationException_IfComponentOfTheSameTypeWasAdded_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    SimulationObject simObject = simulation.SpawnObject();
                    simObject.AddComponent<TestComponent>();
                    simObject.AddComponent<TestComponent>();
                });
            });
            simulation.Tick();
        }

        #endregion

        #region SimulationObject.RemoveComponent

        [TestMethod]
        public void SimulationObject_RemoveComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Spawn new object by ingesting spawn object command
            DateTime commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
            tickSnapshot.Commands.Add(spawnObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);

            simulation.Tick();

            // Get object
            SimulationObject simObject = simulation.GetObjects().First();

            // Invoke RemoveComponent
            Assert.ThrowsException<SimulationException>(
                () => simObject.RemoveComponent<TestComponent>()
            );
        }

        [TestMethod]
        public void SimulationObject_RemoveComponent_RemovesSimulationComponent_SendsRemoveComponentCommand_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            RemoveComponentSimulationCommand command = null;
            simulation.CommandCreated += cmd =>
            {
                if (cmd is RemoveComponentSimulationCommand)
                {
                    command = (RemoveComponentSimulationCommand)cmd;
                }
            };
            SimulationObject simObject = null;
            TestComponent testComponent = null;
            simulation.EnqueueAction(() => {
                simObject = simulation.SpawnObject();
                testComponent = simObject.AddComponent<TestComponent>();
            });
            simulation.Tick(); // Executes action and sends the command

            // Remove component
            simulation.EnqueueAction(() => simObject.RemoveComponent<TestComponent>());
            simulation.Tick(); // Executes action and sends the command

            // Check if was removed
            Assert.IsFalse(simObject.GetComponents().Contains(testComponent));

            // Check if command was sent
            Assert.IsNotNull(command);
            Assert.AreEqual(simObject.Id, command.ObjectId);
            Assert.AreEqual(simulation.GetComponentTypeHash<TestComponent>(), command.ComponentTypeHash);
        }
        #endregion

        #region SimulationObject.TryGetComponent
        [TestMethod]
        public void SimulationObject_TryGetComponent_ReturnsTrue_ProvidesSimulationComponent_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = null;
            TestComponent testComponent = null;

            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                testComponent = simObject.AddComponent<TestComponent>();
            });
            simulation.Tick();

            // Get component
            Assert.IsTrue(simObject.TryGetComponent<TestComponent>(out TestComponent comp));
            Assert.IsNotNull(comp);
        }
        #endregion

        #region SimulationObject.GetComponents
        [TestMethod]
        public void SimulationObject_GetComponents_ReturnsSimulationComponents_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();
            simulation.RegisterComponentType<OtherTestComponent>();

            SimulationObject simObject = null;
            TestComponent testComponent = null;
            OtherTestComponent testComponent2 = null;

            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                testComponent = simObject.AddComponent<TestComponent>();
                testComponent2 = simObject.AddComponent<OtherTestComponent>();
            });
            simulation.Tick();

            // Get component
            var components = simObject.GetComponents();
            Assert.IsTrue(components.Contains(testComponent));
            Assert.IsTrue(components.Contains(testComponent2));
        }

        #endregion

        #region SimulationObject.GetOrAddComponent
        [TestMethod]
        public void SimulationObject_GetOrAddComponent_AddsSimulationComponent_IfNotAdded_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = null;
            TestComponent comp1 = null;
            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                comp1 = simObject.GetOrAddComponent<TestComponent>();
            });
            simulation.Tick();

            Assert.IsNotNull(comp1);

            // Get component
            var comp2 = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual(comp1, comp2); // same object
        }

        [TestMethod]
        public void SimulationObject_GetOrAddComponent_ReturnsSimulationComponent_IfWasAddedBefore_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = null;
            TestComponent comp1 = null;
            simulation.EnqueueAction(() =>
            {
                simObject = simulation.SpawnObject();
                comp1 = simObject.AddComponent<TestComponent>();
            });
            simulation.Tick();

            Assert.IsNotNull(comp1);

            // Get or add component
            var comp2 = simObject.GetOrAddComponent<TestComponent>();
            Assert.IsNotNull(comp2);
            Assert.AreEqual(comp1, comp2); // same object
        }

        [TestMethod]
        public void SimulationObject_GetOrAddComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            simulation.EnqueueAction(() =>
            {
                Assert.ThrowsException<SimulationException>(() =>
                {
                    SimulationObject simObject = new SimulationObject(simulation, logger, 123);
                    TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
                });
            });
            simulation.Tick();
        }
        #endregion

        #region Simulation.IngestCommand
        #region Simulation.IngestCommand(AddComponentSimulationCommand)
        [TestMethod]
        public void Simulation_IngestCommand_AddComponentSimulationCommand_AddsSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new Simulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Create object
            DateTime commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
            tickSnapshot.Commands.Add(spawnObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Add component
            commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var addComponentCommand = new AddComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
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
            var simulation = new Simulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Create object
            DateTime commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var spawnObjectCommand = new SpawnObjectSimulationCommand( 123);
            var tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
            tickSnapshot.Commands.Add(spawnObjectCommand);
            simulation.IngestTickSnapshot(tickSnapshot);
            simulation.Tick();

            Assert.IsTrue(simulation.HasObject(spawnObjectCommand.ObjectId));

            // Add component
            commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var addComponentCommand = new AddComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
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
            commandTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(simulation.IncomingCommandDelayMs); // So that we don't have to wait
            var removeComponentCommand = new RemoveComponentSimulationCommand(123, simulation.GetComponentTypeHash<TestComponent>());
            tickSnapshot = new SimulationTickSnapshot() { TickTime = commandTime };
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