using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Exceptions;
using Fenrir.Multiplayer.LiteNet;
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Tests.Fixtures;
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
        private const int TestTimeout = 1000;

        [TestMethod, Timeout(TestTimeout)]
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

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_ConnectsToFenrirServer_WithLiteNetProtocol()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol(27018);
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
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

        
        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_ConnectsToFenrirServer_WithServerInfoService()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol(27018);
            fenrirServer.AddInfoService(8080);
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_ConnectsToFenrirServer_WithCustomConnectionRequestHandler()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            TaskCompletionSource<object> connectionRequestTcs = new TaskCompletionSource<object>();
            fenrirServer.SetConnectionRequestHandler<CustomConnectionRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                connectionRequestTcs.SetResult(true);
                return Task.FromResult(new ConnectionResponse(true));
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await connectionRequestTcs.Task;
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenCustomConnectionRequestHandlerRejects()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.SetConnectionRequestHandler<CustomConnectionRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                return Task.FromResult(new ConnectionResponse(false, "test_reason"));
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();

            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
            Assert.AreEqual(connectionResponse.Reason, "test_reason");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenCustomConnectionRequestHandlerThrowsException()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();


            fenrirServer.SetConnectionRequestHandler<CustomConnectionRequestData>(connectionRequest =>
            {
                throw new InvalidOperationException("test_exception");
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();

            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_FailsToConnectToFenrirServer_WhenRequestDataSerializationFails()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();


            fenrirServer.SetConnectionRequestHandler<RequestDataFailingToDeserialize>(connectionRequest =>
            {
                return Task.FromResult(ConnectionResponse.Successful);
            });
            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();

            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, fenrirClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequest_SendsRequest()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            TaskCompletionSource<TestRequest> requestTcs = new TaskCompletionSource<TestRequest>();
            fenrirServer.AddRequestHandler(new TcsRequestHandler<TestRequest>(requestTcs));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            fenrirClient.Peer.SendRequest(new TestRequest() { Value = "test_value" });

            TestRequest request = await requestTcs.Task;

            Assert.AreEqual(request.Value, "test_value");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequest_SendsRequest_WithRequestTypeFactory()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            TaskCompletionSource<TestRequest> requestTcs = new TaskCompletionSource<TestRequest>();
            fenrirServer.AddRequestHandler(new TcsRequestHandler<TestRequest>(requestTcs));

            bool didInvokeFactory = false;
            fenrirServer.AddSerializableTypeFactory<TestRequest>(() =>
            {
                didInvokeFactory = true;
                return new TestRequest();
            });

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            fenrirClient.Peer.SendRequest(new TestRequest() { Value = "test_value" });

            TestRequest request = await requestTcs.Task;

            Assert.AreEqual(request.Value, "test_value");

            Assert.IsTrue(didInvokeFactory);
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_SendsRequestWithResponse()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                Assert.AreEqual("request_test", request.Value);
                return new TestResponse() { Value = "response_test" };
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            var response = await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });

            Assert.AreEqual(response.Value, "response_test");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerFails()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                throw new InvalidOperationException("test");
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () => 
            {
                await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerReturnsNull()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return null;
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_SendsRequestWithAsyncResponse()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                Assert.AreEqual("request_test", request.Value);
                return Task.FromResult(new TestResponse() { Value = "response_test" });
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            var response = await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });

            Assert.AreEqual(response.Value, "response_test");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_ThrowsRequestFailedException_IfAsyncRequestHandlerFails()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return Task.FromException<TestResponse>(new InvalidOperationException("test"));
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_ThrowsRequestFailedException_IfAsyncRequestHandlerReturnsNull()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return Task.FromResult<TestResponse>(null);
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddLiteNetProtocol();
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task FenrirClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerTimesOut()
        {
            using var logger = new TestLogger();
            using var fenrirServer = new FenrirServer(logger);
            fenrirServer.AddLiteNetProtocol();
            fenrirServer.AddInfoService();

            fenrirServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(async request =>
            {
                await Task.Delay(1000);
                return new TestResponse() { Value = "response_test" }; // Should not get here
            }));

            await fenrirServer.Start();

            Assert.AreEqual(ServerStatus.Running, fenrirServer.Status, "server is not running");

            using var fenrirClient = new FenrirClient(logger);
            fenrirClient.AddProtocol(new LiteNetProtocolConnector() { RequestTimeoutMs = 100 });
            var connectionResponse = await fenrirClient.Connect("http://127.0.0.1:8080");

            Assert.AreEqual(ConnectionState.Connected, fenrirClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestTimeoutException>(async () =>
            {
                await fenrirClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }

        #region Fixtures
        class CustomConnectionRequestData : IByteStreamSerializable
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
            public void Deserialize(IByteStreamReader reader)
            {
                throw new InvalidOperationException("Error in byte stream serializable");
            }

            public void Serialize(IByteStreamWriter writer)
            {
            }
        }

        class TestRequest : IRequest, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }

        class TestResponse : IResponse, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }


        class TestRequestWithResponse : IRequest<TestResponse>, IByteStreamSerializable
        {
            public string Value;

            public void Deserialize(IByteStreamReader reader)
            {
                Value = reader.ReadString();
            }

            public void Serialize(IByteStreamWriter writer)
            {
                writer.Write(Value);
            }
        }

        class TcsRequestHandler<TRequest> : IRequestHandler<TRequest> 
            where TRequest : IRequest
        {
            private TaskCompletionSource<TRequest> _tcs;

            public TcsRequestHandler(TaskCompletionSource<TRequest> tcs)
            {
                _tcs = tcs;
            }

            public void HandleRequest(TRequest request, IServerPeer peer)
            {
                _tcs.SetResult(request);
            }
        }

        class TestRequestResponseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            private Func<TRequest, TResponse> _callback;

            public TestRequestResponseHandler(Func<TRequest, TResponse> callback)
            {
                _callback = callback;
            }

            public TResponse HandleRequest(TRequest request, IServerPeer peer)
            {
                return _callback(request);
            }
        }

        class TestAsyncRequestResponseHandler<TRequest, TResponse> : IRequestHandlerAsync<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            private Func<TRequest, Task<TResponse>> _callback;

            public TestAsyncRequestResponseHandler(Func<TRequest, Task<TResponse>> callback)
            {
                _callback = callback;
            }

            public async Task<TResponse> HandleRequestAsync(TRequest request, IServerPeer peer)
            {
                return await _callback(request);
            }
        }
        #endregion

    }
}
