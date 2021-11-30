using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Unit.Rooms
{
    [TestClass]
    public class RoomManagerTests
    {
        [TestMethod]
        public async Task TestRoomManager_CreatesRoomUsingRoomFactoryMethod()
        {
            bool didJoin = false;
            bool didLeave = false;
            TestRoom room = null;

            var serverMock = new Mock<INetworkServer>();

            var serverPeerMock = new Mock<IServerPeer>();
            serverPeerMock.Setup(peer => peer.Id).Returns("test_peer");

            var logger = new TestLogger();

            Action<IServerPeer, string> onJoin = (peer, token) => {
                Assert.AreEqual(serverPeerMock.Object, peer);
                Assert.AreEqual("test_token", token);
                didJoin = true;
            };

            Action<IServerPeer> onLeave = (peer) => {
                Assert.AreEqual(serverPeerMock.Object, peer);
                didLeave = true;
            };

            ServerRoomManager<TestRoom>.CreateRoomHandler roomFactoryMethod = (peer, roomId, token) => {
                room = new TestRoom(logger, "test_room", onJoin, onLeave);
                return room;
            };

            var roomManager = new ServerRoomManager<TestRoom>(roomFactoryMethod, logger, serverMock.Object);

            var joinRequestHandler = (IRequestHandlerAsync<RoomJoinRequest, RoomJoinResponse>)roomManager;
            var leaveRequestHandler = (IRequestHandlerAsync<RoomLeaveRequest, RoomLeaveResponse>)roomManager;

            // Join the room
            var joinResponse = await joinRequestHandler.HandleRequestAsync(new RoomJoinRequest("test_room", "test_token"), serverPeerMock.Object);
            Assert.IsTrue(joinResponse.Success);
            Assert.IsTrue(didJoin);

            Assert.AreEqual(1, room.RoomPeers.Count());

            // Leave the room
            var leaveResponse = await leaveRequestHandler.HandleRequestAsync(new RoomLeaveRequest("test_room"), serverPeerMock.Object);
            Assert.IsTrue(leaveResponse.Success);
            Assert.IsTrue(didLeave);
        }

        [TestMethod]
        public async Task TestRoomManager_CreatesRoomUsingRoomFactoryClass()
        {
            bool didJoin = false;
            bool didLeave = false;

            var serverMock = new Mock<INetworkServer>();

            var serverPeerMock = new Mock<IServerPeer>();
            serverPeerMock.Setup(peer => peer.Id).Returns("test_peer");

            var logger = new TestLogger();

            Action<IServerPeer, string> onJoin = (peer, token) => {
                Assert.AreEqual(serverPeerMock.Object, peer);
                Assert.AreEqual("test_token", token);
                didJoin = true;
            };

            Action<IServerPeer> onLeave = (peer) => {
                Assert.AreEqual(serverPeerMock.Object, peer);
                didLeave = true;
            };

            var roomFactory = new TestRoomFactory(logger, onJoin, onLeave);

            var roomManager = new ServerRoomManager<TestRoom>(roomFactory, logger, serverMock.Object);

            var joinRequestHandler = (IRequestHandlerAsync<RoomJoinRequest, RoomJoinResponse>)roomManager;
            var leaveRequestHandler = (IRequestHandlerAsync<RoomLeaveRequest, RoomLeaveResponse>)roomManager;

            // Join the room
            var joinResponse = await joinRequestHandler.HandleRequestAsync(new RoomJoinRequest("test_room", "test_token"), serverPeerMock.Object);
            Assert.IsTrue(joinResponse.Success);
            Assert.IsTrue(didJoin);

            // Leave the room
            var leaveResponse = await leaveRequestHandler.HandleRequestAsync(new RoomLeaveRequest("test_room"), serverPeerMock.Object);
            Assert.IsTrue(leaveResponse.Success);
            Assert.IsTrue(didLeave);
        }

        #region Test Fixtures
        class TestRoom : ServerRoom
        {
            private Action<IServerPeer, string> _onPeerJoin;
            private Action<IServerPeer> _onPeerLeave;

            public IEnumerable<IServerPeer> RoomPeers => Peers.Values;

            public TestRoom(ILogger logger, 
                string roomId, 
                Action<IServerPeer, string> onPeerJoin = null,
                Action<IServerPeer> onPeerLeave = null)
                : base(logger, roomId)
            {
                _onPeerJoin = onPeerJoin;
                _onPeerLeave = onPeerLeave;
            }

            protected override void OnPeerJoin(IServerPeer peer, string token)
            {
                _onPeerJoin?.Invoke(peer, token);
            }

            protected override void OnPeerLeave(IServerPeer peer)
            {
                _onPeerLeave?.Invoke(peer);
            }
        }

        class TestRoomFactory : IServerRoomFactory<TestRoom>
        {
            private Action<IServerPeer, string> _onPeerJoin;
            private Action<IServerPeer> _onPeerLeave;

            private readonly ILogger _logger;

            public TestRoomFactory(ILogger logger, Action<IServerPeer, string> onPeerJoin = null, Action<IServerPeer> onPeerLeave = null)
            {
                _logger = logger;
                _onPeerJoin = onPeerJoin;
                _onPeerLeave = onPeerLeave;
            }

            public TestRoom Create(IServerPeer peer, string roomId, string token)
            {
                return new TestRoom(_logger, roomId, _onPeerJoin, _onPeerLeave);
            }
        }

        #endregion
    }
}
