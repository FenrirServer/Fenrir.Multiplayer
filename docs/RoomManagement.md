# Room Management

This section explains basics of Room Management in Fenrir Multiplayer.

Previous Section: [Custom Serializer](/CustomSerializer.md)

## Basics of Room Management

Fenrir Server comes with room management out of the box.

![Fenrir Multiplayer](/docs/images/ServerRooms.png)

Rooms allows to write isolated, single-threaded gameplay logic without worrying about performance or concurrency.

Depending on a game genre, a room can represent a game session, a match (MOBA, TCG, etc) a zone (MMO) etc.

## Room quick start

To add room management on the server, you need to define a room class. 
This class will receive players when they join or leave. 
We provide an abstract base class `ServerRoom` that we recommend to extend. Alternatively, you can study it and implement `IServerRoom` interface directly.

Each room has a unique id and is **created when first user joins**, and is **destroyed when last player leaves**.

**Basic room example:**

```csharp
using Fenrir.Multiplayer.Network;
using Fenrir.Multiplayer.Rooms;

class MyRoom : ServerRoom
{
    public MyRoom(ILogger logger, string roomId)
        : base(logger, roomId)
    {
        // Constructor...
    }

    protected override void OnPeerJoin(IServerPeer peer, string token)
    {
        Logger.Info("Player joined: " + peer.Id);
    }

    
    protected override void OnPeerLeave(IServerPeer peer)
    {
        Logger.Info("Player left: " + peer.Id);
    }
}
```

**Server Room Management**:

In order to use the room management, you can use the extension method:

```csharp
using Fenrir.Multiplayer.Server;
using Fenrir.Multiplayer.Rooms;

...

private MyRoom CreateMyRoom(IServerPeer peer, string roomId, string joinToken)
{
    // "peer" is a reference to a connected peer that wants to initiate this room.
    // You do not need to add them to the room manually.
    return new MyRoom(_logger, roomId);
}


using var networkServer = new NetworkServer();
networkServer.AddRooms(CreateMyRoom);
```

**Client Room Management:**

```csharp
using Fenrir.Multiplayer.Client;
using Fenrir.Multiplayer.Rooms;

...

using var networkClient = new NetworkClient();
networkClient.JoinRoom("room_id_1");

...

networkClient.LeaveRoom("room_id_1");
```

## Room Action Queue

One of the most advantages to the room approach, is ability to write single-threaded code that operates on an isolated group of players.

To make that easier, we provide basic methods for running and scheduling execution on the room thread.

**Important: Each request invocation happens on a separate thread.** To execute game logic from a multi-threaded code such as Request Handlers, use room scheduling.

Server code:

```csharp
class MoveRequest : IRequest { ... }

class MyRoom : ServerRoom
{
    ...

    protected override void OnPeerJoin(IServerPeer peer, string token)
    {
        peer.PeerData = this; // Store room in PeerData. You can store any custom object reference in PeerData.
    }

    public void DispatchMoveRequest(MoveRequest request, IServerPeer peer)
    {
        // Move player, or other gameplay logic
    }
}

class MoveRequestHandler : IRequestHandler<MoveRequest>
{
    public void HandleRequest(MoveRequest request, IServerPeer peer)
    {
        // Multi-threaded code! Dispatch to the room logic
        
        // Get room
        var room = (MyRoom)peer.PeerData;

        if(room == null)
            return; // player not in a room
        
        // Execute method on a room thread.
        room.Execute(() => room.DispatchMoveRequest(request, peer));
    }
}
```

You can also execute asynchronous tasks or schedule delayed execution on the room thread:

```csharp

// Executes MovePlayer() on the room thread and waits until method returns a value.
MoveResult result = await room.ExecuteAsync(() => room.MovePlayer(request, peer));

// Executes delayed action. Great for timers!
room.Schedule(() => FinishRound(), TimeSpan.FromSeconds(10));
```

Next Section: [Logging](/Logging.md)
