using System;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Single-threaded game event loop
    /// </summary>
    public interface IActionQueue
    {
        /// <summary>
        /// Indicates if action queue is processing actions
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Runs event loop. Starts processing actions in the queue
        /// </summary>
        void Run();

        /// <summary>
        /// Stops action queue
        /// </summary>
        void Stop();

        /// <summary>
        /// Adds action to the queue
        /// </summary>
        /// <param name="action">Callback</param>
        void Enqueue(Action action);

        /// <summary>
        /// Schedules action with a specified delay
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="delayMs">Delay, in MS</param>
        void Schedule(Action action, double delayMs);

        /// <summary>
        /// Schedules action with a specified delay
        /// </summary>
        /// <param name="action">Callback</param>
        /// <param name="delayMs">Delay</param>
        void Schedule(Action action, TimeSpan delay);
    }
}