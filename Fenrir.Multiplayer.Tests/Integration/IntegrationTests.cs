using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
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
                new LiteNetProtocolListener(){ Port = 27015 }
            });

            // Start service
            var serverInfoService = new ServerInfoService(fenrirServerMock.Object);
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
            var fenrirServer = new FenrirServer();
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            var fenrirClient = new FenrirClient();
            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27015))
                }
            };

            await fenrirClient.Connect(serverInfo);

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
        }
    }
}
