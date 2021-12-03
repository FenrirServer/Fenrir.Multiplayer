# Custom Serializer

This section explains how to use a third-party serialization library such as MessagePack or Protobuf, or create a completely custom serializer.

Previous Section: [Serializing Custom Types](/SerializingCustomTypes.md)

## Using Third Party Serializer

You can add custom serializer for all unknown types by implementing non-generic `ITypeSerializer`.

This is useful when you want to use a third-party serialization library such as ProtoBuf or MessagePack.

Plugin packages for those are coming soon!

**Example custom serializer using `DataContractSerializer`**:

Note: `DataContractSerializer` does not provide performance sufficient for most games. Provided only as an example.

```csharp
using Fenrir.Multiplayer.Serialization;

class CustomSerializer : ITypeSerializer
{
    public void Serialize(object data, IByteStreamWriter byteStreamWriter)
    {
        // Write bytes into a memory stream
        using var memoryStream = new MemoryStream();

        var dataContractSerializer = new System.Runtime.Serialization.DataContractSerializer(data.GetType());
        dataContractSerializer.WriteObject(memoryStream, data);

        byte[] objectBytes = memoryStream.ToArray();
        byteStreamWriter.WriteBytesWithLength(objectBytes);
    }

    public object Deserialize(Type type, IByteStreamReader byteStreamReader)
    {
        // Copy bytes from the network buffer into a memory stream
        byte[] bytes = byteStreamReader.ReadBytesWithLength();
        using var memoryStream = new MemoryStream(bytes);

        // Deserialize memory stream into an object
        var dataContractSerializer = new System.Runtime.Serialization.DataContractSerializer(type);
        object data = dataContractSerializer.ReadObject(memoryStream);
        return data;
    }
}

[DataContract]
class RoundStartEvent : IEvent
{
    [DataMember]
    public string PlayerName;

    [DataMember]
    public int Health;
}

...

// Add to the serializer
var serializer = new NetworkSerializer();
serializer.AddTypeSerializer(new CustomSerializer());

// Server
var networkServer = new NetworkServer(new FenrirLogger(), serializer);
networkServer.PeerConnected += (sender, e) => e.Peer.SendEvent(new RoundStartEvent() { PlayerName = "Player", Health=100 });

// Client
var networkClient = new NetworkClient(new UnityLogger(), serializer);
await networkClient.Connect(...);
```

Next Section: [Room Management](/RoomManagement.md)
