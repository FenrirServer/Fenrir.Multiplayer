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
