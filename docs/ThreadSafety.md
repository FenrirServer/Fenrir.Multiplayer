# Thread Safety

This section explains basics of a threading model used by Fenrir Multiplayer 

Previous Section: [Docker Setup](/DockerSetup.md)

## Multithreading Basics

Fenrir allows to build high-performance multi-threaded applications with very basic understanding of multi-threading.

### Fenrir SDK methods are thread-safe, unless specified otherwise

You can call any regular operations such as `peer.SendRequest`, `peer.SendEvent` from any thread.

### Network Client SDK is single-threaded

`NetworClient` events will be invoked from a single thread, running on a default Synchronization Context:

1. In Unity, it will be invoked on Unity main thread.
2. In a standalone .NET app, it will be invoked on a thread pool. 

You can also set `NetworkClient.AutoPollEvents` to `false` and invoke `NetworkClient.PollEvents` manually. This allows you to trigger NetworkClient events from any thread/whenever you want. 

### Network Server Threading Model

Each Connection Handler / Request Handler invocation happens on a separate thread, specific to a Server Peer.

### Room Threading Model

Room SDK simplifies multiplayer development. Each Room has it's own event loop baked by a single thread.

However, incoming requests handlers must still be scheduled to be run on a room.

Please refer to [Room Management](/RoomManagement.md)  for the further details.

**⚠️ In other words, Request Handlers are multi-threaded, and Rooms are single-threaded. ⚠️**

## Asynchronous Methods

Request handlers can be synchronous and asynchronous. Asynchronous request handlers will use default Synchronization Context.

Please refer to [Networking Basics](/NetworkingBasics.md) for further details.



Next Section: [Life Cycle](/LifeCycle.md)
