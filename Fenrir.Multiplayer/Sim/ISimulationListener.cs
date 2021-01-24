using Fenrir.Multiplayer.Sim.Command;
using System.Collections.Generic;

namespace Fenrir.Multiplayer.Sim
{
    /// <summary>
    /// Simulation command listener
    /// </summary>
    public interface ISimulationListener
    {
        /// <summary>
        /// Invoked when simulation attempts to send a single outgoing command
        /// </summary>
        /// <param name="command">Command</param>
        void OnSendCommand(ISimulationCommand command);

        /// <summary>
        /// Invoked when simulation attempts to send an outgoing command batch
        /// </summary>
        /// <param name="commands">Command batch</param>
        void OnSendCommands(IEnumerable<ISimulationCommand> commands);
    }
}
