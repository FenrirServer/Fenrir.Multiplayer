using Fenrir.Multiplayer.Logging;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Utility
{
    /// <summary>
    /// Simple extension class that runs fire-and-forget Task, without losing error information.
    /// </summary>
    static class TaskExtensions
    {
        public static void FireAndForget(this Task task, ILogger logger)
        {
            task.ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    logger.Error(t.Exception.ToString());
                }
            });
        }
    }
}
