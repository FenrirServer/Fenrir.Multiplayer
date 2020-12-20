using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public async Task ServerInfoService_ReturnsServerInfo()
        {
            var fenrirServerMock = new Mock<IFenrirServerInfoProvider>();
            fenrirServerMock.Setup(server => server.Status).Returns(ServerStatus.Running);
            fenrirServerMock.Setup(server => server.ServerId).Returns("test_id");
            fenrirServerMock.Setup(server => server.Listeners).Returns(new IProtocolListener[] { 
                new LiteNetProtocolListener(){ BindPort = 27015 }
            });

            // Start service
            using var serverInfoService = new ServerInfoService(fenrirServerMock.Object);
            await serverInfoService.Start();

            // Connect
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync(new Uri($"http://127.0.0.1:{serverInfoService.Port}/"));

            Assert.IsTrue(result.IsSuccessStatusCode, $"bad status code from {nameof(ServerInfoService)}: {result.StatusCode}");
            string response = await result.Content.ReadAsStringAsync();
            Assert.IsNotNull(response, "response is null");
            Assert.IsFalse(response == string.Empty, "empty response");

            // Deserialize
            ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(response);

            Assert.AreEqual("test_id", serverInfo.ServerId);
            Assert.AreEqual(1, serverInfo.Protocols.Length, "incorrect number of protocols");
            Assert.AreEqual(ProtocolType.LiteNet, serverInfo.Protocols[0].ProtocolType, "incorrect protocol type");

            // Get connection data
            var connectionData = serverInfo.Protocols[0].GetConnectionData(typeof(LiteNetProtocolConnectionData)) as LiteNetProtocolConnectionData;

            Assert.IsNotNull(connectionData, "connection data is null");
            Assert.AreEqual(27015, connectionData.Port);
        }

        [TestMethod]
        public async Task FenrirClient_ConnectsToFenrirServer_WithLiteNetProtocol()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol(27018);
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();
            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27018))
                }
            };

            await fenrirClient.Connect(serverInfo);

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
        }

        
        [TestMethod]
        public async Task FenrirClient_ConnectsToFenrirServer_WithServerInfoService()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol(27018);
            fenrirServer.AddInfoService(8080);
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();
            await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
        }

        [TestMethod]
        public async Task FenrirClient_ConnectsToFenrirServer_WithCustomConnectionRequestHandler()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            TaskCompletionSource<object> connectionRequestTcs = new TaskCompletionSource<object>();
            fenrirServer.SetConnectionRequestHandler<CustomRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                connectionRequestTcs.SetResult(true);
                return Task.FromResult(new ConnectionResponse(true));
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not disconnected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await connectionRequestTcs.Task;
        }

        [TestMethod]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenCustomConnectionRequestHandlerRejects()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.SetConnectionRequestHandler<CustomRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                return Task.FromResult(new ConnectionResponse(false, "test_reason"));
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();


            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
            Assert.AreEqual(connectionResponse.Reason, "test_reason");
        }


        [TestMethod]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenCustomConnectionRequestHandlerThrowsException()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();


            fenrirServer.SetConnectionRequestHandler<CustomRequestData>(connectionRequest =>
            {
                throw new InvalidOperationException("test_exception");
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();


            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        [TestMethod]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenRequestDataSerializationFails()
        {
            using var fenrirServer = new FenrirServer();
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();


            fenrirServer.SetConnectionRequestHandler<RequestDataFailingToDeserialize>(connectionRequest =>
            {
                return Task.FromResult(ConnectionResponse.Successful);
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();

            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        #region Fixtures
        class CustomRequestData : IByteStreamSerializable
        {
            public string Token;

            public void Deserialize(IByteStreamReader reader)
            {
                Token = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Token);
            }
        }


        class RequestDataFailingToDeserialize : IByteStreamSerializable
        {
            public string Token;

            public void Deserialize(IByteStreamReader reader)
            {
                throw new InvalidOperationException("Error in byte stream serializable");
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Token);
            }
        }

        #endregion

    }
}
