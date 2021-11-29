using Fenrir.Multiplayer.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Rooms
{
    /// <summary>
    /// Single threaded event loop
    /// </summary>
    class ActionQueue : IActionQueue, IDisposable
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// List of scheduled actions
        /// </summary>
        private Queue<Action> _actions = new Queue<Action>();

        /// <summary>
        /// Indicates if event loop is running
        /// </summary>
        private volatile bool _isRunning = false;

        /// <summary>
        /// Set to true when object is disposed
        /// </summary>
        private volatile bool _isDisposed = false;

        /// <summary>
        /// Running lock
        /// </summary>
        private object _isRunningLock = new object();

        /// <summary>
        /// TaskCompletionSource that completes 
        /// when action is enqueued and we need to signal
        /// Run() loop to continue dispatching
        /// </summary>
        private TaskCompletionSource<bool> _actionEnqueuedTcs = null;

        /// <summary>
        /// Thread safe property
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_isRunningLock)
                {
                    return _isRunning;
                }
            }
            private set
            {
                lock (_isRunningLock)
                {
                    _isRunning = value;
                }
            }
        }

        /// <summary>
        /// Creates new Action Queue
        /// </summary>
        /// <param name="logger">Logger</param>
        public ActionQueue(ILogger logger)
            : this()
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates new Action Queue
        /// </summary>
        public ActionQueue()
        {
        }

        /// <inheritdoc/>
        public async void Run()
        {
            if(_isDisposed)
            {
                throw new InvalidOperationException("Failed to run Action queue after it was disposed");
            }

            IsRunning = true;

            while(IsRunning)
            {
                // Invoke all enqueued actions
                while(IsRunning && TryDequeueAction(out Action action))
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch(Exception e)
                    {
                        _logger?.Error("Uncaught exception in scheduled action {0}: {1}", action.ToString(), e.ToString());
                    }
                }

                // No actions left in the queue, wait for one to be enqueued
                if(IsRunning)
                {
                    // Wait for action to be enqueued
                    _actionEnqueuedTcs = new TaskCompletionSource<bool>();
                    await _actionEnqueuedTcs.Task;
                }
            }

            // Event loop has stopped
        }

        /// <summary>
        /// Attempts to dequeue an action from the queue in a thread-safe manner
        /// </summary>
        /// <param name="action">Action that is dequeued</param>
        /// <returns>True if queue was not empty and action is dequeued, otherwise false</returns>
        private bool TryDequeueAction(out Action action)
        {
            action = null;

            lock (_actions)
            {
                if (_actions.Count > 0)
                {
                    action = _actions.Dequeue();
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <inheritdoc/>
        public void Enqueue(Action action)
        {
            lock(_actions)
            {
                _actions.Enqueue(action);
            }

            // Signal event loop to continue execution 
            _actionEnqueuedTcs?.TrySetResult(false);
        }

        /// <inheritdoc/>
        public async void Schedule(Action action, double delayMs)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs));

            if(_isDisposed)
            {
                return;
            }

            Enqueue(action);
        }

        /// <inheritdoc/>
        public async void Schedule(Action action, TimeSpan delay)
        {
            await Task.Delay(delay);

            if (_isDisposed)
            {
                return;
            }

            Enqueue(action);
        }

        #region IDisposable Implementation
        public void Dispose()
        {
            _isDisposed = true;

            Stop();

            lock (_actions)
            {
                _actions.Clear();
            }
        }
        #endregion
    }
}
