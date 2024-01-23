# Life Cycle

This section explains the Life Cycle of NetworkServer and NetworkClient objects.

Previous Section: [Thread Safety](/ThreadSafety.md)

## Server and Client Lifecycle

Both `NetworkClient` and `NetworkServer` implement `IDisposable` and should be used within the `using` statement`, or be disposed properly.

⚠️ Failure to dispose a NetworkClient properly could lead to issues such as unclosed socket.

Unity Example:

```csharp
class MyBehaviour : MonoBehaviour
{
    private NetworkClient _client;

    public void Start()
    {
        _client = new NetworkClient();
    }

    ...

    public void Destroy()
    {
        _client.Dispose();
    }
}
```

## Server Shutdown event

Built-in server template includes a Shutdown mechanism that can be used for server draining / rolling updates.

Example server with draining: 

```csharp
// Listen for SIGTERM - invoked when Docker container is shutting down
AppDomain.CurrentDomain.ProcessExit += (_, _) => application.Shutdown(0).Wait();

class Application
{
    public async Task Shutdown(int exitCode)
    {
        // Notify players they have to leave...
        ...

        // Wait for all peers to disconnect
        while(_peers.Count > 0)
        {
            await Task.Delay(1000); // wait 1 seconds
            _logger.Info($"Waiting for players to leave: " + DateTime.UtcNow);
        }
    }    
}
```
