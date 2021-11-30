![Fenrir Multiplayer](/docs/images/FenrirLogo512.png)

# Fenrir Multiplayer Library

[![Tests](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/test/badge.svg)](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/test.yml)
[![Deploy](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/deploy/badge.svg)](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/deploy.yml)
[![License](https://img.shields.io/github/license/FenrirServer/Fenrir.Multiplayer)](https://github.com/FenrirServer/Fenrir.Multiplayer/blob/master/LICENSE.txt)
[![Issues](https://img.shields.io/github/issues/FenrirServer/Fenrir.Multiplayer)](https://github.com/FenrirServer/Fenrir.Multiplayer/issues)]
[![NuGet](https://img.shields.io/nuget/v/Fenrir.Multiplayer)](https://www.nuget.org/packages/Fenrir.Multiplayer/)
![Unity Package Manager](https://img.shields.io/npm/v/org.fenrirserver.multiplayer/latest?registry_uri=https%3A%2F%2Fupm.fenrirserver.org)]

Fenrir is a platform for building server-authoritative real-time multiplayer games with C# and .NET.

This library provides supports for building multiplayer games using Unity on the client-side and .NET and Docker on the server-side.

It is optimized for real-time multiplayer and provides fast reliable UDP layer and a basic serialization engine. It also supports various serialization engines and plug-ins.

Fenrir provides a great balance between performance and ease of development and makes it extremely quick to build multiplayer games of any genre.

## Quick Start

### Installation

This package can be installed using Unity Package from `https://upm.fenrirserver.org` registry.

![Fenrir Multiplayer](/docs/images/UnityPackageManager.png)

1. In Unity, open **Edit** → **Project Settings** → **Package Manager** and add a **Scoped Registry** using URL: `https://upm.fenrirserver.org`
2. Open **Window** → **Package Manager** and switch to **Packages: My Registries**. Select **Fenrir Multiplayer** and click **Install**

### Server Project

Fenrir Multiplayer Unity Package comes with a server template that is recommended (but not required) to use. 

To generate a server project, click **Window** → **Fenrir**  → **Open Server Project**.

![Server Project](/docs/images/OpenServerProject.png)

Editor script will generate and open a .NET Solution in the folder next to Assets:

```
📂**MyGame**
 ┣ 📂Assets
 ┣ 📂Packages
 ┣ 📂Library
 ┣ 📂ProjectSettings
 ┣ 📂**Server**   ← this folder is generated 
   ┣ 📂 **MyGame.Server**   ← Server .NET Project 
   ┣ 📂 **MyGame.Shared**   ← Server and Client Shared Library
   ┣ 📜 **ServerApplication.sln**   ← Server solution file
   ┗ 📜 **Dockerfile**   ← Dockerfile that defines how the docker image for your server is built
```

Once the solution file is open, you can build and run the server.

### Client-Server Basics

Fenrir Multiplayer allows you to build server-authoritative multiplayer games with Unity and .NET. 

Unlike most networking solutions for Unity, Fenrir allows (and encourages) to separate client and server logic. 
Data contracts and common code can be shared between client and server.

**Example Server:**

Note: Server template that comes with Unity Package includes basic hello world example for the server, which is also provided here.

```csharp
using var networkServer = new NetworkServer() { BindPort = 27016 };
networkServer.Start();
```

**Example Client:**

```csharp
using var networkClient = new NetworkClient();
var connectionResponse = await networkClient.Connect("http://localhost:27016");
if(connectionResponse.Success)
    Debug.Log("Connected!");
else
    Debug.Log("Failed to connect:" + connectionResponse.Reason);
```

### Networking Basics

Fenrir Multiplayer uses basic messaging primitives: **Requests**, **Responses**, and **Events**.

![Request Response Event](/docs/images/RequestResponseEvent.png)

**Request**
Very much like HTTP, requests are sent from the client to a server. **Request** can optionally have a **Response**.
Requests usually represent an action that client wants server to perform. For example, invoke a specific server method (similar to RPC), or update a client-authoritative state.
Another example, is request to send someone a chat message.

**Response**
Response is always sent from server to client. Response objects always have a request that triggered them, but not every requests require a response.
For example, response might contain information about successful (or unsuccessful) chat message delivery.

**Event**
Events are messages asynchoronously sent from a server to a client. 
Events usually update client state asynchronously when client might or might not expect. For example, they may represent an incoming chat message.

#### Request and Event Handlers

Fenrir provides a strictly typed mechanism of defining messages such as Request, Responses and Events. This means that Requests, Responses and Events are defined as C# classes.
Messages only represent data sent over the wire. In other words, they are **Data Transfer Objects**.
It is programmers job to define how Requests are handled by the server (and Events are handled by the client).

In order to do that, you can define a Request Handler or Event Handler.

**Example Request** (client+server shared code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

class HelloRequest : IRequest, IByteStreamSerializable
{
    public string Name;

    // You can read about message Serialization in the next section

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Name);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Name = reader.ReadString();
    }
}
```

**Example server Request handler** (server code)

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;

// Request Handler
class HelloRequestHandler : IRequestHandler<HelloRequest>
{
    public void HandleRequest(HelloRequest request, IServerPeer peer)
    {
        Console.WriteLine("Hello, " + request.Name); // prints: "Hello, World"
    }
}

...

// Server initialization
using var networkServer = new NetworkServer(); // Default BindPort is 27016
networkServer.AddRequestHandler<HelloRequest>(new HelloRequestHandler());
networkServer.Start();
```

**Example client sending a Request** (client code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Client;

using var networkClient = new NetworkClient();
var connectionResponse = await networkClient.Connect("http://127.0.0.1:27016");

// Send request
networkClient.Peer.SendRequest(new HelloRequest() { Name = "World" });

// Server prints "Hello, World"
```

---

**Example Request with Response** (client+server shared code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

class PingResponse : IResponse, IByteStreamSerializable
{
    public string ResponseText;

    public void Deserialize(IByteStreamReader reader)
    {
        ResponseText = reader.ReadString();
    }

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(ResponseText);
    }
}

class PingRequest : IRequest<PingResponse>, IByteStreamSerializable
{
    public string Name;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Name);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Name = reader.ReadString();
    }
}
```

**Example server Request handler with Response** (server code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;

// Request Handler
class PingRequestHandler : IRequestHandler<PingRequest, PingResponse>
{
    public PingResponse HandleRequest(PingRequest request, IServerPeer peer)
    {
        return new PingResponse(){ ResponseText = "Hello, " + request.Name };
    }
}

...

// Server initialization
using var networkServer = new NetworkServer(); // Default BindPort is 27016
networkServer.AddRequestHandler<PingRequest, PingResponse>(new PingRequestHandler());
networkServer.Start();
```

**Example client sending a Request with a Response** (client code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Client;

using var networkClient = new NetworkClient();
await networkClient.Connect("http://127.0.0.1:27016");

// Send request
var response = await networkClient.Peer.SendRequest<PingRequest, PingResponse>(new PingRequest() { Name = "World" });
Debug.Log(response.ResponseText); // prints "Hello, World"
```

---

**Example Event** (client+server shared code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Serialization;

class PingEvent : IEvent, IByteStreamSerializable
{
    public string Name;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Name);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Name = reader.ReadString();
    }
}
```

**Example server sending Event** (server code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Server;

using var networkServer = new NetworkServer();
networkServer.PeerConnected += (sender, e) =>
{
    // When new peer connects, send them an event
    e.Peer.SendEvent(new TestEvent() { Value = "Mr.Server" });
};
networkServer.Start();
```

**Example client Event Handler** (client code):

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Client;

class PingEventHandler : IEventHandler<PingEvent>
{
    public void OnReceiveEvent(PingEvent evt)
    {
        Debug.Log("Hello from " + evt.Name);
    }
}

...

// Client initialization
using var networkClient = new NetworkClient();
networkClient.AddEventHandler<PingEvent>(new PingEventHandler());
await networkClient.Connect("http://127.0.0.1:27016");
// After a successful connection, client prints "Hello from Mr.Server"
```

### Serialization

By default, Fenrir Multiplayer comes with a byte stream serialization mechanism for messages and data objects.
It is simple but powerful: it allows explicitly serializing messages into directly into a byte buffer that is used to write into a socket.

In order to use byte stream serializables, your messages should implement `IByteStreamSerializable`:

```csharp
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Network;

class MyEvent : IEvent, IByteStreamSerializable
{
    // Field that we want to write into a buffer
    public string Name;

    // Invoked during serialization
    public void Serialize(IByteStreamWriter writer)
    {
        // Write string into a byte buffer
        writer.Write(Name);
    }

    // Invoked during deserialization
    public void Deserialize(IByteStreamReader reader)
    {
        // Read string from the byte buffer
        Name = reader.ReadString();
    }
}
```

You can write simple types such as strings, integers, enums etc. 
You can also write nested types that implement IByteStreamSerializable:

```csharp
using Fenrir.Multiplayer.Serialization;
using Fenrir.Multiplayer.Network;

class Player : IByteStreamSerializable
{
    public string Name;
    public int Health;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Name);
        writer.Write(Health);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Name = reader.ReadString();
        Health = reader.ReadInt();
    }
}

class MyEvent : IEvent, IByteStreamSerializable
{
    public Player Name;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Player);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Player = reader.Read<Player>();
    }
}
```

Arrays, dictionaries and lists are also supported:

```csharp
class RoundStartEvent : IEvent, IByteStreamSerializable
{
    public Player[] Players;
    public Dictionary<string, Player> PlayersByName;
    public List<Player> PlayerList;

    public void Serialize(IByteStreamWriter writer)
    {
        writer.Write(Players);
        writer.Write(PlayersByName);
        writer.Write(PlayerList);
    }

    public void Deserialize(IByteStreamReader reader)
    {
        Players = reader.Read<Player[]>();
        PlayersByName = reader.Read<Dictionary<string, Player>>();
        PlayerList = reader.Read<List<Player>>();
    }
}
```


TODO: Type serializer binding

TODO: Serialization using other libraries coming soon!


### Room Management
TODO


### Running in Docker
TODO

### Installing directly via NuGet

We recommend that you use a server project template that comes with the Unity Package. If you do not wish to use it, or would like to use this library directly, you can install it using NuGet package:

```bash
dotnet add package Fenrir.Multiplayer 
```

Or install it using NuGet Window: [Fenrir.Multiplayer](https://www.nuget.org/packages/Fenrir.Multiplayer/)

## Contributing

For problems with this library, please open a GitHub issue or a pull-request. 

For issues with Fenrir Cloud, please contact customer support or your account manager.

## License

Fenrir.Multiplayer is an open source software, licensed under the terms of MIT license. See [LICENSE.txt](/LICENSE.txt) for details.
