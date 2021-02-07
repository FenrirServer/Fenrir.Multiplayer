using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Sim;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Integration.Sim
{
    static class SimulationTestFixtures
    {
        public static async Task WaitForNextTick(this Simulation simulation)
        {
            var tickTcs = new TaskCompletionSource<bool>();
            simulation.EnqueueLateAction(() => tickTcs.SetResult(true));
            await tickTcs.Task;
        }

        public static async Task WaitForTicks(this Simulation simulation, int numTicks)
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

    class TestComponent : SimulationComponent
    {
    }
}
