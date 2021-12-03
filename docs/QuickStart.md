# Quick Start

This section contains Quick Start Guide for Unity Package of the Fenrir Multiplayer SDK.

Previous Section: [Installation](/Installation.md)

## Server Project

Fenrir Multiplayer Unity Package comes with a server template that is recommended (but is not strictly required) to use.

To generate a server project, click **Window** → **Fenrir** → **Open Server Project**.

![Server Project](/images/OpenServerProject.png)

If a project server has never been generated, dialogue window will open asking to generate the package. Select **Generate**.

![Generate Server Project](/images/GenerateServerProject.png)

Editor script will generate and open a .NET Solution in the folder next to Assets:

```
📂MyGame
 ┣ 📂Assets
 ┣ 📂Packages
 ┣ 📂Library
 ┣ 📂ProjectSettings
 ┣ 📁Server                      ← Generated Server folder
   ┣ 📂 MyGame.Server            ← Server .NET Project 
   ┣ 📂 MyGame.Shared            ← Server and Client Shared Library
   ┣ 📄 ServerApplication.sln    ← Server solution file
   ┗ 📄 Dockerfile               ← Dockerfile that defines how the docker image for your server is built
```

Once the solution file is open, you can build and run the server.

## Example code

Fenrir Multiplayer allows you to build server-authoritative multiplayer games with Unity and .NET.

Unlike most networking solutions for Unity, Fenrir allows (and encourages) to separate client and server logic.
Data contracts and common code can be shared between client and server.

**Example Server:**

Note: Server template that comes with Unity Package includes basic hello world example for the server, which is provided here.

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

Next Section: [Networking Basics](/NetworkingBasics.md)