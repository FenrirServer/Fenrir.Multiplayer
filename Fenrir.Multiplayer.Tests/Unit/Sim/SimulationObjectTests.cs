using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Sim.Exceptions;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
        [TestMethod]
        public void SimulationObject_AddComponent_AddsSimulationComponent_UsingDefaultConstructor_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            TestComponent testComponent = simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentNotRegistered_UsingComponentReference_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            var comp = new TestComponent("test2");
            simObject.AddComponent(comp);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void SimulationObject_AddComponent_ThrowsArgumentException_IfComponentOfTheSameTypeWasAdded_WhenServerSim()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);

            SimulationObject simObject = simulation.CreateObject();
            simObject.AddComponent<TestComponent>();
            simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_AddComponent_ThrowsSimulationException_UsingDefaultConstructor_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, 123);
            simObject.AddComponent<TestComponent>();
        }

        [TestMethod, ExpectedException(typeof(SimulationException))]
        public void SimulationObject_AddComponent_ThrowsSimulationException_UsingComponentReference_WhenClientSim()
        {
            var logger = new TestLogger();
            var simulationViewMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationViewMock = new Mock<ISimulationView>();
            var simulationServerViewMock = new Mock<ISimulationServerView>();
            var simulation = new ServerSimulation(logger, simulationViewMock.Object, simulationServerViewMock.Object);
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
            var simulationClientMock = new Mock<ISimulationView>();
            var simulation = new Simulation(logger, simulationClientMock.Object);
            simulation.RegisterComponentType<TestComponent>();

            SimulationObject simObject = new SimulationObject(simulation, 123);
            TestComponent comp1 = simObject.GetOrAddComponent<TestComponent>();
        }
        #endregion

    }
}
