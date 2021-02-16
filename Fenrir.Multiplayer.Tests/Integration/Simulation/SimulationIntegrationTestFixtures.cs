using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Simulation;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Integration.Simulation
{
    static class SimulationTestFixtures
    {
        public static async Task WaitForNextTick(this NetworkSimulation simulation)
        {
            var tickTcs = new TaskCompletionSource<bool>();
            simulation.EnqueueLateAction(() => tickTcs.SetResult(true));
            await tickTcs.Task;
        }

        public static async Task WaitForTicks(this NetworkSimulation simulation, int numTicks)
        {
            int numTick = 0;

            var tickTcs = new TaskCompletionSource<bool>();
            simulation.EnqueueLateAction(() =>
            {
                numTick++;

                if (numTick == numTicks)
                {
                    tickTcs.SetResult(true);
                }
            });

            await tickTcs.Task;
        }
    }
}
