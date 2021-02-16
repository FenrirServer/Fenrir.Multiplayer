using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Logging;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Tests.Fixtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests.Integration.Rooms
{
    [TestClass]
    public class RoomIntegrationTests
    {
        [TestMethod]
        public async Task TestJoinLeaveRoom()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };
            networkServer.AddRooms<TestRoom>((peer, roomId, token) => new TestRoom(logger, "test_room_id"));
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            var eventTcs = new TaskCompletionSource<TestEvent>();
            var testEventHandler = new TestEventHandler(eventTcs);

            using var networkClient = new NetworkClient(logger);
            networkClient.AddEventHandler<TestEvent>(testEventHandler);
            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27018))
                }
            };

            // Connect to server
            await networkClient.Connect(serverInfo);
            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");

            // Join room
            var joinResult = await networkClient.JoinRoom("test_room_id");
            Assert.IsTrue(joinResult.Success);

            // Receive room event
            await eventTcs.Task;

            // Leave room
            var leaveResult = await networkClient.LeaveRoom("test_room_id");
            Assert.IsTrue(leaveResult.Success);

        }

        #region Fixtures
        class TestRoom : ServerRoom
        {
            public TestRoom(ILogger logger, string roomId)
                : base(logger, roomId)
            {
            }

            protected override void OnPeerJoin(IServerPeer peer, string token)
            {
                peer.SendEvent(new TestEvent() { Test = "test" });
            }

            protected override void OnPeerLeave(IServerPeer peer)
            {
            }
        }

        class TestEvent : IEvent, IByteStreamSerializable
        {
            public string Test;

            public void Deserialize(IByteStreamReader reader)
            {
                Test = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Test);
            }
        }

        class TestEventHandler : IEventHandler<TestEvent>
        {
            private TaskCompletionSource<TestEvent> _receiveTcs;

            public TestEventHandler(TaskCompletionSource<TestEvent> receiveTcs)
            {
                _receiveTcs = receiveTcs;
            }

            public void OnReceiveEvent(TestEvent evt)
            {
                _receiveTcs.SetResult(evt);
            }
        }

        #endregion
    }
}
