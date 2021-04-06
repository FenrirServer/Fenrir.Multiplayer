using Fenrir.Multiplayer.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Utility
{
    /// <summary>
    /// Simple extension class that runs fire-and-forget Task, without losing error information.
    /// </summary>
    static class TaskExtensions
    {
        /// <summary>
        /// Runs the task and does not await it.
        /// Any failed task will log an exception using given <see cref="ILogger"/>
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="logger">Logger to log an exception if task fails</param>
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

        /// <summary>
        /// Runs the task, or times out after a given period
        /// </summary>
        /// <typeparam name="TResult">Result of the asynchronous operation</typeparam>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task that completes with a given result, or throws a <seealso cref="TimeoutException"/></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        /// <summary>
        /// Runs the task, or times out after a given period
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Task that completes, or throws a <seealso cref="TimeoutException"/></returns>
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));

                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
