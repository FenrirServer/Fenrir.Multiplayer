# Peer Object

This section explains Server and Client Peer objects

Previous Section: [Connection](Connection.md)

## Multiplayer Peers

**Peer** is simply a player that's being connected to a server.

**Client Peer** is a player connected on the client side. In the client code, there is always only one Client Peer.

**Server Peer** is a player connected on the server side. One server could have thousands of connected ServerPeers.

## Identifying Server Peer

There are several ways of uniquely identifying each Peer.

## Connection Request Data

If you set up a custom Connection Request Handler, you can validate custom Connection Request Data object and access it later using `IServerPeer.ConnectionRequestData` property:

```csharp
// Use with NetworkServer.SetConnectionRequestHandler<>
class MyConnectionRequestData
{
    public string Name;

    ...
}

...

void HandleRequest(PlayerMoveRequest request, IServerPeer peer)
{
    string name = peer.ConnectionRequestData.Name; 
}
```

See also: [Using Custom Connection Request Handler](Connection.md#custom-connection-request)

## Server Peer Data

Each peer has an arbitrary `PeerData` object that you can assign. The `PeerData` object can work as a tag or an id, or may contain any reference of your choice.

A common patern is to assign custom "Player" object to `PeerData`:

```csharp
class MyPlayer 
{
    public string Name;
}

using var networkServer = new NetworkServer() { BindPort = 27016 };
networkServer.PeerConnected += (sender, e) => 
{
    e.Peer.PeerData = new MyPlayer(){ Name = "name" };
};
networkServer.
```


### Unique Peer Id

On the `NetworkClient`, you can set a property named `ClientId`. This property will match `Peer.Id` on the server:

```csharp
using var networkClient = new NetworkClient(logger) { ClientId = "player1" };
await networkClient.Connect("http://127.0.0.1:27016");
```

When accessing server peer on the server, it will now have a given id:

```csharp
using var networkServer = new NetworkServer() { BindPort = 27016 };
networkServer.PeerConnected += (sender, e) => {
    Console.WriteLine(e.Peer.Id); // prints: "player1"
};
networkServer.Start();
```

**⚠ Warning: By default, Peer Id is not validated for uniqueness or in any way.**

In many cases, clients "disconnect" by timing out.
Depending on the timeout, it is possible that the same client (with the same Peer Id) will attempt to connect while previous connection on the server has not timed out yet.
It is up to a developer to decide how to handle this case. 

A common solution is to check if peer with the same id is already connected and prompt to kick them. However you may want to keep both peers connected at the same time and not rely on Peer Id for communication.

**Rooms, however, do rely on the Peer Id to be unique.**
By default, room will not allow anyone with the same Peer Id to join while peer with the same ID is still in that room. 

You can override this behavior by overriding `ServerRoom.OnBeforePeerJoin` and kicking the previous player:

```csharp
class MyRoom : ServerRoom
{
    ... 

    protected override RoomJoinResponse OnBeforePeerJoin(IServerPeer peer, string token)
    {
        // Get already connected peer
        if(Peers.TryGetValue(peer.Id, out IServerPeer connectedPeer))
        {
            // Kick, send them an example "KickEvent" that you might want to create
            peer.SendEvent(new KickEvent() { Reason = "Connected from elsewhere" });
            RemovePeer(connectedPeer);
        }

        // Allow new peer to join
        return new RoomJoinResponse(true);
    }

    ...
}
```

See also: [RoomManagement](RoomManagement.md)


Next Section: [Reliability](Reliability.md)
