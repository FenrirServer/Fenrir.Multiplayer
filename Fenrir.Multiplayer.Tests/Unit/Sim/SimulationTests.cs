using Fenrir.Multiplayer.Sim;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit.Sim
{
    [TestClass]
    public class SimulationTests
    {
        [TestMethod]
        public void Simulation_AddPlayer_InvokesPlayerHandler()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            simulation.AddPlayer("test_player");

            playerHandlerMock.Verify(handler => handler.PlayerAdded(simulation, It.IsAny<SimulationObject>()));
        }

        [TestMethod]
        public void Simulation_RemovePlayer_InvokesPlayerHandler()
        {
            var logger = new TestLogger();
            var playerHandlerMock = new Mock<ISimulationPlayerHandler>();
            var simulation = new Simulation(logger, playerHandlerMock.Object);

            simulation.AddPlayer("test_player");

            simulation.RemovePlayer("test_player");

            playerHandlerMock.Verify(handler => handler.PlayerRemoved(simulation, It.IsAny<SimulationObject>()));
        }

        #region Test Fixtures
        #endregion
    }
}
