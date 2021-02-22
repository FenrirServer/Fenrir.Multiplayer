using Fenrir.Multiplayer.Simulation;
using Fenrir.Multiplayer.Simulation.Command;
using Fenrir.Multiplayer.Simulation.Data;
using Fenrir.Multiplayer.Simulation.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Fenrir.Multiplayer.Tests.Unit.Simulation
{
    [TestClass]
    public class SimulationObjectTests
    {
        #region SimulationObject.AddComponent

        [TestMethod]
        public void SimulationObject_AddComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };

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
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Spawn new object by ingesting spawn object command
            var spawnObjectCommand = new SpawnObjectSimulationCommand(123);
            var tickSnapshot = new SimulationTickSnapshot(1, DateTime.UtcNow);
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
        public void SimulationObject_TryGetComponent_ReturnsTrue_ProvidesSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            Assert.IsTrue(simObject.TryGetComponent(typeof(TestComponent), out SimulationComponent comp));
            Assert.IsNotNull(comp);
            Assert.IsInstanceOfType(comp, typeof(TestComponent));
        }

        [TestMethod]
        public void SimulationObject_TryGetComponentGeneric_ReturnsTrue_ProvidesSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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

        #region SimulationObject.GetComponent
        [TestMethod]
        public void SimulationObject_GetComponent_ReturnsSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            SimulationComponent comp = simObject.GetComponent(typeof(TestComponent));
            Assert.IsNotNull(comp);
            Assert.IsInstanceOfType(comp, typeof(TestComponent));
        }

        [TestMethod]
        public void SimulationObject_GetComponentGeneric_ReturnsSimulationComponent()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            TestComponent comp = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(comp);
        }

        #endregion

        #region SimulationObject.GetComponents
        [TestMethod]
        public void SimulationObject_GetComponents_ReturnsSimulationComponents_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = true };
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
            var simulation = new NetworkSimulation(logger) { IsAuthority = false };
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


    }
}