# Connection

This section explains details of client to server connections.

Previous Section: [Networking](NetworkingBasics.md)

## Connection Basics

Fenrir Multiplayer uses UDP as a primary protocol for communication between client and server.
Since UDP is a connectionless protocol, a concept of "connection" is proided by the SDK.

## Server Info

In order to establish a server-client connection, a client need to be provided a **Server Info** object.

Client can either obtain that object from an http(s) endpoint, or be directly provided with it.

By default, all Fenrir Servers expose the **Server Info http endpoint** which binds to the same port as UDP.
Server Info endpoint is very useful for local testing and connecting to any specific server instance directly.

In production setups, very often the Server Info will be provided by the matchmaking server. In that case, TCP port does not need to be exposed.

** TODO: Endpoint vs UDP infographic **

**Example connecting via http endpoint**:

Server code:

```csharp
using var networkServer = new NetworkServer() { BindPort = 27016 }; // Binds both UDP and TCP (http)
networkServer.Start();
```

You can now test the http endpoint:

```bash
$ curl localhost:27016
{
    "Hostname":"127.0.0.1",
    "ServerId":"8284ba32-4b36-4b31-89ad-ae3cb412d3ab",
    "Protocols": [
        { 
            "ProtocolType": "LiteNet",
            "ConnectionData": {
                "Port":27016,
                "IPv6Mode":"DualMode"
            }
        }
    ]
}
```

Connect using http endpoint:

```csharp
using var networkClient = new NetworkClient();
var connectionResponse = await networkClient.Connect("http://localhost:27016");
```

Connect using ServerInfo object directly:

```csharp
// Example code, in the real-world this info should come from a matchmaker or a master server (e.g. service discovery)
var serverInfo = new ServerInfo()
{
    Hostname = "127.0.0.1",
    ServerId = "8284ba32-4b36-4b31-89ad-ae3cb412d3ab", // Should match the server GUID
    Protocols = new ProtocolInfo[]
    {
        // Add allowed protocols
        new ProtocolInfo(ProtocolType.LiteNet, new LiteNetProtocolConnectionData(27016))
    }
};

using var networkClient = new NetworkClient(logger);
await networkClient.Connect(serverInfo);
```

## Custom Connection Request

Custom connection request can be used to provide additional means of authentication, validation of connecting players etc.

First, define your custom connection request object. This should be defined in your **Shared** data models project.

```csharp
class MyConnectionRequestData : IByteStreamSerializable
{
    public string UserName;
    public string AuthToken;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(UserName);
        writer.Write(AuthToken);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        UserName = reader.ReadString();
        AuthToken = reader.ReadString();
    }
}
```

Next, add your custom connection request handler in the server code:

```csharp
// Custom asynchronous Connection Request Handler
async Task<ConnectionResponse> MyConnectionRequestHandler(IServerConnectionRequest<MyConnectionRequestData> connectionRequest)
{
    string userName = connectionRequest.Data.UserName;
    string authToken = connectionRequest.Data.AuthToken;

    bool result = await ValidateUserAsync(userName, authToken); // e.g. send to some web api for validation
    
    if(result)
        return new ConnectionResponse(true);
    else
        return new ConnectionResponse(false, "Failed to check auth token");
}

// Create server
using var networkServer = new NetworkServer() { BindPort = 27016 };

// Set connection request handler
networkServer.SetConnectionRequestHandler<MyConnectionRequestData>(MyConnectionRequestHandler);

//Start
networkServer.Start();
```

Finally, you can connect to your server using custom connection request:

```csharp
// Create custom connection request data
var connectionRequest = new MyConnectionRequestData() { UserId = "test_user", AuthToken = "test_token" };

// Create client
using var networkClient = new NetworkClient();

// Connect using custom data
var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016", connectionRequest);
```

Next Section: [Peer Object](PeerObject.md)
Table of Contents: [Documentation Home](DocumentationIndex.md)
