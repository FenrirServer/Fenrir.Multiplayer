using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fenrir.Multiplayer.Tests.Unit.Network
{
    [TestClass]
    public class EventHandlerMapTests
    {
        [TestMethod]
        public void EventHandlerMap_RemoveEventHandler_RemovesRegisteredHandler()
        {
            var eventHandlerMap = new EventHandlerMap(new EventBasedLogger());
            var eventHandler = new TestEventHandler();

            eventHandlerMap.AddEventHandler<TestEvent>(eventHandler);
            eventHandlerMap.RemoveEventHandler<TestEvent>(eventHandler);

            eventHandlerMap.OnReceiveEvent(MessageWrapper.WrapEvent(new TestEvent(), 0, MessageFlags.None, MessageDeliveryMethod.ReliableOrdered));

            Assert.AreEqual(0, eventHandler.ReceivedCount);
        }

        [TestMethod]
        public void EventHandlerMap_RemoveEventHandler_AllowsHandlerToBeAddedAgain()
        {
            var eventHandlerMap = new EventHandlerMap(new EventBasedLogger());
            var eventHandler = new TestEventHandler();

            eventHandlerMap.AddEventHandler<TestEvent>(eventHandler);
            eventHandlerMap.RemoveEventHandler<TestEvent>(eventHandler);
            eventHandlerMap.AddEventHandler<TestEvent>(eventHandler);

            eventHandlerMap.OnReceiveEvent(MessageWrapper.WrapEvent(new TestEvent(), 0, MessageFlags.None, MessageDeliveryMethod.ReliableOrdered));

            Assert.AreEqual(1, eventHandler.ReceivedCount);
        }

        [TestMethod]
        public void EventHandlerMap_RemoveEventHandler_ThrowsWhenHandlerNotRegistered()
        {
            var eventHandlerMap = new EventHandlerMap(new EventBasedLogger());

            Assert.ThrowsException<EventHandlerException>(() => eventHandlerMap.RemoveEventHandler<TestEvent>(new TestEventHandler()));
        }

        class TestEvent : IEvent
        {
        }

        class TestEventHandler : IEventHandler<TestEvent>
        {
            public int ReceivedCount { get; private set; }

            public void OnReceiveEvent(TestEvent evt)
            {
                ReceivedCount++;
            }
        }
    }
}
