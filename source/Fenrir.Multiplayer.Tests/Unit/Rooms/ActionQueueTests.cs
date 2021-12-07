using Fenrir.Multiplayer.Rooms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Rooms
{
    [TestClass]
    public class ActionQueueTests
    {
        [TestMethod]
        public void ActionQueue_Run_RunsActionQueue()
        {
            var actionQueue = new ActionQueue();
            actionQueue.Run();

            bool didInvoke = false;
            actionQueue.Enqueue(() => 
            {
                didInvoke = true;
            });

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public void ActionQueue_Run_InvokesAction_WhenEnqueuedBeforeRun()
        {
            var actionQueue = new ActionQueue();

            bool didInvoke = false;
            actionQueue.Enqueue(() =>
            {
                didInvoke = true;
            });

            actionQueue.Run();

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public void ActionQueue_Stop_StopsActionQueue()
        {
            var actionQueue = new ActionQueue();
            actionQueue.Run();

            bool didInvoke1 = false;
            bool didInvoke2 = false;
            actionQueue.Enqueue(() =>
            {
                didInvoke1 = true;
            });

            Assert.IsTrue(didInvoke1);

            actionQueue.Stop();

            actionQueue.Enqueue(() =>
            {
                didInvoke2 = true;
            });

            Assert.IsFalse(didInvoke2);
        }

        [TestMethod, Timeout(1000)]
        public async Task ActionQueue_Schedule_InvokesAction_AfterDelay()
        {
            var actionQueue = new ActionQueue();
            actionQueue.Run();

            bool didInvoke = false;
            actionQueue.Schedule(() =>
            {
                didInvoke = true;
            }, 100);

            await Task.Delay(150);

            Assert.IsTrue(didInvoke);
        }

        [TestMethod]
        public void ActionQueue_Enqueue_LogsError_WhenCallbackThrows()
        {
            bool didLog = false;

            var logger = new EventBasedLogger();
            logger.Logged += (level, format, args) =>
            {
                Assert.AreEqual(LogLevel.Error, level);
                didLog = true;
            };

            var actionQueue = new ActionQueue(logger);
            actionQueue.Run();

            actionQueue.Enqueue(() =>
            {
                throw new InvalidOperationException();
            });

            Assert.IsTrue(didLog);
        }

        [TestMethod, Timeout(1000)]
        public async Task ActionQueue_Schedule_LogsError_WhenCallbackThrows()
        {
            bool didLog = false;

            var logger = new EventBasedLogger();
            logger.Logged += (level, format, args) =>
            {
                Assert.AreEqual(LogLevel.Error, level);
                didLog = true;
            };

            var actionQueue = new ActionQueue(logger);
            actionQueue.Run();

            actionQueue.Schedule(() =>
            {
                throw new InvalidOperationException();
            }, 100);

            await Task.Delay(150);

            Assert.IsTrue(didLog);
        }
    }
}
