using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Command;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationObjectTests
    {
        #region SimulationObject.AddComponent

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_AddComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, logger, 123);
            simObject.AddComponent<TestComponent>();
        }

        [TestMethod]
        public void SimulationObject_AddComponent_AddsSimulationComponent_SendsAddComponentCommand_UsingDefaultFactory_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

            Assert.AreEqual("test", testComponent.Value);

            // Get component
            var component = simObject.GetComponent<TestComponent>();
            Assert.IsNotNull(component);
            Assert.AreEqual("test", component.Value);

            // Verify command was sent
            Assert.IsTrue(listener.TryGetCommand<AddComponentSimulationCommand>(out var command));
            Assert.AreEqual(simulation.GetComponentTypeHash<TestComponent>(), command.ComponentTypeHash);
            Assert.AreEqual(component.Object.Id, command.ObjectId);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingDefaultConstructor_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };

            SimulationObject simObject = simulation.SpawnObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingComponentReference_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };

            SimulationObject simObject = simulation.SpawnObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentOfTheSameTypeWasAdded_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };

            SimulationObject simObject = simulation.SpawnObject();
            simObject.AddComponent<TestComponent>();
            simObject.AddComponent<TestComponent>();
        }

        #endregion

        #region SimulationObject.RemoveComponent

        [TestMethod]
        public void SimulationObject_RemoveComponent_ThrowsSimulationException_WhenNoAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            // Spawn new object by ingesting spawn object command
            var spawnObjectCommand = new SpawnObjectSimulationCommand(DateTime.UtcNow, 123);
            simulation.IngestCommand(spawnObjectCommand);

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
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

            // Remove component
            simObject.RemoveComponent<TestComponent>();

            // Check if was removed
            Assert.IsFalse(simObject.GetComponents().Contains(testComponent));

            // Check if command was sent
            Assert.IsTrue(listener.TryGetCommand<AddComponentSimulationCommand>(out var command));
            Assert.AreEqual(simObject.Id, command.ObjectId);
            Assert.AreEqual(simulation.GetComponentTypeHash<TestComponent>(), command.ComponentTypeHash);
        }
        #endregion

        #region SimulationObject.TryGetComponent
        [TestMethod]
        public void SimulationObject_TryGetComponent_ReturnsTrue_ProvidesSimulationComponent_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();

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
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();
            simulation.RegisterComponentType<OtherTestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
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
        public void SimulationObject_GetOrAddComponent_AddsSimulationComponent_IfNotAdded_WhenHasAuthority()
        {
            var logger = new TestLogger();
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
            TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
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
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = true };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = simulation.SpawnObject();
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
            var listener = new TestSimulationListener();
            var simulation = new Simulation(listener, logger) { IsAuthority = false };
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, logger, 123);
            TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
        }
        #endregion

    }
}
