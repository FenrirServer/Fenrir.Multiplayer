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


        [TestMethod]
        public async Task TestAllPeersLeave_RemovesRoom()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };

            bool roomTerminated = false;

            networkServer.AddRooms<TestRoom>((peer, roomId, token) =>
            {
                var room = new TestRoom(logger, "test_room_id");
                room.Terminated += (sender, e) => roomTerminated = true;
                return room;
            });

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27018))
                }
            };

            using var networkClient1 = new NetworkClient(logger);
            using var networkClient2 = new NetworkClient(logger);

            // Connect to server
            await networkClient1.Connect(serverInfo);
            await networkClient2.Connect(serverInfo);
            Assert.AreEqual(ConnectionState.Connected, networkClient1.State, "client is not connected");
            Assert.AreEqual(ConnectionState.Connected, networkClient2.State, "client is not connected");

            // Join the same room
            var joinResult1 = await networkClient1.JoinRoom("test_room_id");
            var joinResult2= await networkClient2.JoinRoom("test_room_id");
            Assert.IsTrue(joinResult1.Success);
            Assert.IsTrue(joinResult2.Success);

            // Leave room
            var leaveResult1 = await networkClient1.LeaveRoom("test_room_id");
            var leaveResult2 = await networkClient2.LeaveRoom("test_room_id");
            Assert.IsTrue(leaveResult1.Success);
            Assert.IsTrue(leaveResult2.Success);

            // Test that room was destroyed
            Assert.IsTrue(roomTerminated);
        }


        [TestMethod]
        public async Task TestAllPeersDisconnect_RemovesRoom()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };

            TaskCompletionSource<bool> roomTerminatedTcs = new TaskCompletionSource<bool>();

            networkServer.AddRooms<TestRoom>((peer, roomId, token) =>
            {
                var room = new TestRoom(logger, "test_room_id");
                room.Terminated += (sender, e) => roomTerminatedTcs.SetResult(true);
                return room;
            });

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27018))
                }
            };

            using var networkClient1 = new NetworkClient(logger);
            using var networkClient2 = new NetworkClient(logger);

            // Connect to server
            await networkClient1.Connect(serverInfo);
            await networkClient2.Connect(serverInfo);
            Assert.AreEqual(ConnectionState.Connected, networkClient1.State, "client is not connected");
            Assert.AreEqual(ConnectionState.Connected, networkClient2.State, "client is not connected");

            // Join the same room
            var joinResult1 = await networkClient1.JoinRoom("test_room_id");
            var joinResult2 = await networkClient2.JoinRoom("test_room_id");
            Assert.IsTrue(joinResult1.Success);
            Assert.IsTrue(joinResult2.Success);

            // Disconnect
            networkClient1.Disconnect();
            networkClient2.Disconnect();

            // Wait until room is destroyed
            await roomTerminatedTcs.Task;
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
