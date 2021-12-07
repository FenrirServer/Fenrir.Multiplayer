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
        private const int TestTimeout = 5000;

        [TestMethod, Timeout(TestTimeout)]
        public async Task ServerInfoService_ReturnsServerInfo()
        {
            var protocolListenerMock = new Mock<IProtocolListener>();
            protocolListenerMock.Setup(listener => listener.ProtocolType).Returns(ProtocolType.LiteNet);
            protocolListenerMock.Setup(listener => listener.GetConnectionData())
                .Returns(new LiteNetProtocolConnectionData() { Port = 27018, IPv6Mode = IPv6ProtocolMode.DualMode });

            var networkServerMock = new Mock<IServerInfoProvider>();
            networkServerMock.Setup(server => server.Status).Returns(ServerStatus.Running);
            networkServerMock.Setup(server => server.ServerId).Returns("test_id");
            networkServerMock.Setup(server => server.Listeners).Returns(new IProtocolListener[] { protocolListenerMock.Object });

            // Start service
            using var serverInfoService = new ServerInfoService(networkServerMock.Object);
            serverInfoService.Start(8080);

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
            Assert.AreEqual(27018, connectionData.Port);
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_ConnectsToNetworkServer_WithLiteNetProtocol()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var serverInfo = new ServerInfo()
            {
                Hostname = "127.0.0.1",
                ServerId = "test_id",
                Protocols = new ProtocolInfo[]
                {
                    new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27018))
                }
            };

            await networkClient.Connect(serverInfo);

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
        }

        
        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_ConnectsToNetworkServer_WithServerInfoService()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            await networkClient.Connect("http://127.0.0.1:27018");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_ConnectsToNetworkServer_WithCustomClientId()
        {
            TaskCompletionSource<IServerPeer> serverConnectionTcs = new TaskCompletionSource<IServerPeer>();

            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger) { BindPort = 27018 };

            networkServer.PeerConnected += (sender, e) =>
            {
                serverConnectionTcs.SetResult(e.Peer);
            };
            
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger) { ClientId = "test_id" };
            await networkClient.Connect("http://127.0.0.1:27018");
            var serverPeer = await serverConnectionTcs.Task;

            Assert.IsNotNull(serverPeer);
            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.AreEqual(networkClient.Peer.Id, "test_id", "invalid client peer id after connecting");
            Assert.AreEqual(serverPeer.Id, "test_id", "invalid server peer id after connecting");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_ConnectsToNetworkServer_WithCustomConnectionRequestHandler()
        {
            TaskCompletionSource<CustomConnectionRequestData> peerConnectionRequestDataTcs = new TaskCompletionSource<CustomConnectionRequestData>();

            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);
            networkServer.PeerConnected += (sender, e) =>
            {
                var requestData = (CustomConnectionRequestData)e.Peer.ConnectionRequestData;
                peerConnectionRequestDataTcs.SetResult(requestData);
            };

            TaskCompletionSource<object> connectionRequestTcs = new TaskCompletionSource<object>();
            networkServer.SetConnectionRequestHandler<CustomConnectionRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                connectionRequestTcs.SetResult(true);
                return new ConnectionResponse(true);
            });
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await connectionRequestTcs.Task;

            CustomConnectionRequestData peerConnectionRequestData = await peerConnectionRequestDataTcs.Task;

            Assert.IsNotNull(peerConnectionRequestData);
            Assert.AreEqual("test", peerConnectionRequestData.Token);
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_ConnectsToNetworkServer_WithCustomConnectionRequestHandlerAsync()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            TaskCompletionSource<object> connectionRequestTcs = new TaskCompletionSource<object>();
            networkServer.SetConnectionRequestHandlerAsync<CustomConnectionRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                connectionRequestTcs.SetResult(true);
                return Task.FromResult(new ConnectionResponse(true));
            });
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await connectionRequestTcs.Task;
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_FailsToConnectToNetworkServer_WhenCustomConnectionRequestHandlerAsyncRejects()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.SetConnectionRequestHandlerAsync<CustomConnectionRequestData>(connectionRequest =>
            {
                Assert.AreEqual("test", connectionRequest.Data.Token);
                return Task.FromResult(new ConnectionResponse(false, "test_reason"));
            });
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);

            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, networkClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
            Assert.AreEqual(connectionResponse.Reason, "test_reason");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_FailsToConnectToNetworkServer_WhenCustomConnectionRequestHandlerAsyncThrowsException()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);


            networkServer.SetConnectionRequestHandlerAsync<CustomConnectionRequestData>(connectionRequest =>
            {
                throw new InvalidOperationException("test_exception");
            });
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);

            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, networkClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_FailsToConnectToNetworkServer_WhenRequestDataSerializationFails()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);


            networkServer.SetConnectionRequestHandlerAsync<RequestDataFailingToDeserialize>(connectionRequest =>
            {
                return Task.FromResult(ConnectionResponse.Successful);
            });
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);

            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", new CustomConnectionRequestData() { Token = "test" });

            Assert.AreEqual(ConnectionState.Disconnected, networkClient.State, "client is not disconnected");
            Assert.IsFalse(connectionResponse.Success, "connection was not rejected");
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequest_SendsRequest()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            TaskCompletionSource<TestRequest> requestTcs = new TaskCompletionSource<TestRequest>();
            networkServer.AddRequestHandler(new TcsRequestHandler<TestRequest>(requestTcs));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            networkClient.Peer.SendRequest(new TestRequest() { Value = "test_value" });

            TestRequest request = await requestTcs.Task;

            Assert.AreEqual(request.Value, "test_value");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequest_SendsRequest_WithRequestTypeFactory()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            TaskCompletionSource<TestRequest> requestTcs = new TaskCompletionSource<TestRequest>();
            networkServer.AddRequestHandler(new TcsRequestHandler<TestRequest>(requestTcs));

            bool didInvokeFactory = false;
            networkServer.AddSerializableTypeFactory<TestRequest>(() =>
            {
                didInvokeFactory = true;
                return new TestRequest();
            });

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            networkClient.Peer.SendRequest(new TestRequest() { Value = "test_value" });

            TestRequest request = await requestTcs.Task;

            Assert.AreEqual(request.Value, "test_value");

            Assert.IsTrue(didInvokeFactory);
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_SendsRequestWithResponse()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                Assert.AreEqual("request_test", request.Value);
                return new TestResponse() { Value = "response_test" };
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            var response = await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });

            Assert.AreEqual(response.Value, "response_test");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerFails()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                throw new InvalidOperationException("test");
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () => 
            {
                await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerReturnsNull()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandler(new TestRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return null;
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_SendsRequestWithAsyncResponse()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                Assert.AreEqual("request_test", request.Value);
                return Task.FromResult(new TestResponse() { Value = "response_test" });
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            var response = await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });

            Assert.AreEqual(response.Value, "response_test");
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_ThrowsRequestFailedException_IfAsyncRequestHandlerFails()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return Task.FromException<TestResponse>(new InvalidOperationException("test"));
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_ThrowsRequestFailedException_IfAsyncRequestHandlerReturnsNull()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(request =>
            {
                return Task.FromResult<TestResponse>(null);
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger);
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestFailedException>(async () =>
            {
                await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }


        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkClient_SendRequestResponse_ThrowsRequestFailedException_IfRequestHandlerTimesOut()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.AddRequestHandlerAsync(new TestAsyncRequestResponseHandler<TestRequestWithResponse, TestResponse>(async request =>
            {
                await Task.Delay(1000);
                return new TestResponse() { Value = "response_test" }; // Should not get here
            }));

            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            using var networkClient = new NetworkClient(logger) { RequestTimeoutMs = 100 };
            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            await Assert.ThrowsExceptionAsync<RequestTimeoutException>(async () =>
            {
                await networkClient.Peer.SendRequest<TestRequestWithResponse, TestResponse>(new TestRequestWithResponse() { Value = "request_test" });
            });
        }

        [TestMethod, Timeout(TestTimeout)]
        public async Task NetworkServer_SendEvent_SendsEvent()
        {
            using var logger = new TestLogger();
            using var networkServer = new NetworkServer(logger);

            networkServer.PeerConnected += (sender, e) =>
            {
                e.Peer.SendEvent(new TestEvent() { Value = "event_test" });
            };
            networkServer.Start();

            Assert.AreEqual(ServerStatus.Running, networkServer.Status, "server is not running");

            TaskCompletionSource<TestEvent> tcs = new TaskCompletionSource<TestEvent>();

            using var networkClient = new NetworkClient(logger);
            var eventHandler = new TestEventHandler<TestEvent>(tcs);
            networkClient.AddEventHandler<TestEvent>(eventHandler);

            var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

            Assert.AreEqual(ConnectionState.Connected, networkClient.State, "client is not connected");
            Assert.IsTrue(connectionResponse.Success, "connection rejected");

            var testEvent = await tcs.Task;

            Assert.AreEqual(testEvent.Value, "event_test");
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

        class TestEvent : IEvent, IByteStreamSerializable
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

        class TestEventHandler<TEvent> : IEventHandler<TEvent>
            where TEvent : IEvent
        {
            private TaskCompletionSource<TEvent> _tcs;

            public TestEventHandler(TaskCompletionSource<TEvent> tcs)
            {
                _tcs = tcs;
            }

            public void OnReceiveEvent(TEvent evt)
            {
                _tcs?.SetResult(evt);
            }
        }
        #endregion

    }
}
