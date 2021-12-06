# Logging

This section explains how to enable logging in Fenrir Multiplayer SDK.

Previous Section: [Room Management](/RoomManagement.md)

## Enabling Logging

To enable advanced logging, a class that implements `Fenrir.Multiplayer.Logging.ILogger` can be passed when constructing `INetworkClient` or `INetworkServer`.

In our Unity package, UnityLogger class is available that you can use:

Client code:

```csharp
using var networkClient = new NetworkClient(new UnityLogger());
```

In the .NET server project template, we provide `FenrirLogger` that uses `Microsoft.Extensions.Logging` to log:

Server code:

```csharp
using var networkServer = new NetworkServer(new FenrirLogger());
```

Alternatively, you can also use EventBasedLogger:

```csharp
void OnLogged(LogLevel level, string format, params object[] arguments)
{
    // log your message
}

var eventBasedLogger = new EventBasedLogger();
eventBasedLogger.Logged = OnLogged;
using var networkServer = new NetworkServer(eventBasedLogger);
```

Next Section: [Docker Setup](/DockerSetup.md)

## Message Debugging

When turned on, will include additional text information to each message (Request, Response or Event) when sending over the wire.

This can be useful specifically for debugging issues with unknown message type hashes, etc.

Enable in client (Client will add debug information to all Requests sent to a Server):

```csharp
using var networkClient = new NetworkClient();
var connectionResponse = await networkClient.Connect("http://localhost:27016");
networkClient.Peer.WriteDebugInfo = true;
```

Enable in server (Server will add debug information to all Responses and Events sent back to the specific client):

```csharp
using var networkServer = new NetworkServer();
networkServer.PeerConnected += (sender, e) => {
    e.Peer.WriteDebugInfo = true;
};
networkServer.Start();
```

**⚠ Warning: Enabling Message Debugging negatively affects performance and adds significant amount of data to each UDP message, and could easily bring message size over the MTU.**
You should never use this option in Release builds. Only use when needed.

Without Message Debugging:

```csharp
Unexpected message with type hash 9223372036854775807
```

With Message Debugging:

```csharp
Unexpected message with type hash 9223372036854775807. Message debug info: MessageType=Event, MessageDataType=RoundStartEvent, RequestId=1412 Channel=1, Flags=MessageFlags.IsDebug, DeliveryMethod=MessageDeliveryMethod.ReliableOrdered
```
