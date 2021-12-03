# Custom Type Serialization

This section explains how to serialize custom types like Vector3, Quaternion etc.

Previous Section: [Serialization](/Serialization.md)

## Defining Custom TypeSerializer

You can create define serialize logic for a custom type such as Vector3 in Unity.

Custom serializer must implement `ITypeSerializer<T>` interface as such:

```csharp
using UnityEngine;
using Fenrir.Multiplayer.Serialization;

class Vector3Serializer : ITypeSerializer<Vector3>
{
    public Vector3 Deserialize(IByteStreamReader byteStreamReader)
    {
        float x = byteStreamReader.ReadFloat();
        float y = byteStreamReader.ReadFloat();
        float z = byteStreamReader.ReadFloat();

        return new Vector3(x,y,z);
    }

    public void Serialize(Vector3 data, IByteStreamWriter byteStreamWriter)
    {
        byteStreamWriter.Write(data.x);
        byteStreamWriter.Write(data.y);
        byteStreamWriter.Write(data.z);
    }
}

// Add to the serializer
var serializer = new NetworkSerializer();
serializer.AddTypeSerializer<Vector3>(new Vector3Serializer());

// Server
var networkServer = new NetworkServer(new FenrirLogger(), serializer);

// Client
var networkClient = new NetworkClient(new UnityLogger(), serializer); 
```

Next Section: [Custom Serializer](/CustomSerializer.md)
