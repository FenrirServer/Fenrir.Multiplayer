![Fenrir Multiplayer](/docs/images/FenrirLogo.png)

# Fenrir Multiplayer SDK

[![Tests](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/test.yml/badge.svg)](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/test.yml)
[![Deploy](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/deploy.yml/badge.svg)](https://github.com/FenrirServer/Fenrir.Multiplayer/actions/workflows/deploy.yml)

[![License](https://img.shields.io/github/license/FenrirServer/Fenrir.Multiplayer)](https://github.com/FenrirServer/Fenrir.Multiplayer/blob/master/LICENSE.txt)
[![Issues](https://img.shields.io/github/issues/FenrirServer/Fenrir.Multiplayer)](https://github.com/FenrirServer/Fenrir.Multiplayer/issues)
[![NuGet](https://img.shields.io/nuget/v/Fenrir.Multiplayer)](https://www.nuget.org/packages/Fenrir.Multiplayer/)
![Unity Package Manager](https://img.shields.io/npm/v/org.fenrirserver.multiplayer/latest?registry_uri=https%3A%2F%2Fupm.fenrirserver.org)

Fenrir is a platform for building server-authoritative real-time multiplayer games with C# and .NET.

This library provides supports for building multiplayer games using Unity on the client-side and .NET and Docker on the server-side.

It is optimized for real-time multiplayer and provides fast reliable UDP layer and a basic serialization engine. It also supports various serialization engines and plug-ins.

Fenrir provides a great balance between performance and ease of development and makes it extremely quick to build multiplayer games of any genre.

# Documentation

**Table of Contents**:

- [Documentation Home](docs/DocumentationIndex.md)
- [Installation](docs/Installation.md)
- [Quick Start](docs/QuickStart.md)
- [Networking](docs/NetworkingBasics.md)
  - [Connection](docs/Connection.md)
  - [Peer Object](docs/PeerObject.md)
  - [Reliability](docs/Reliability.md)
- [Serialization](docs/Serialization.md)
  - [Custom Type Serialization](docs/CustomTypeSerialization.md)
  - [Custom Serializer](docs/CustomSerializer.md)
- [Room Management](docs/RoomManagementBasics.md)
- [Logging](docs/Logging.md)
- [Docker Setup](docs/DockerBasics.md)
- [Thread Safety](docs/ThreadSafety.md)
- [Client and Server Lifecycle](docs/Lifecycle.md)
- [FAQ](docs/FAQ.md)

# Quick Start

## Installing Unity Package

This package can be installed using Unity Package from `https://upm.fenrirserver.org` registry.

![Fenrir Multiplayer](/docs/images/UnityPackageManager.png)

1. In Unity, open **Edit** → **Project Settings** → **Package Manager** and add a **Scoped Registry** using URL: `https://upm.fenrirserver.org`
2. Open **Window** → **Package Manager** and switch to **Packages: My Registries**. Select **Fenrir Multiplayer** and click **Install**

## Generating Server Project

Fenrir Multiplayer Unity Package comes with a server template that is recommended (but is not strictly required) to use.

To generate a server project, click **Window** → **Fenrir** → **Open Server Project**.

![Server Project](/docs/images/OpenServerProject.png)

If a project server has never been generated, dialogue window will open asking to generate the package. Select **Generate**.

![Generate Server Project](/docs/images/GenerateServerProject.png)

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

## Connecting to Server

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

# Reference Project

Please check out [TicTacToe](https://github.com/FenrirServer/Examples-TicTacToe/) repository for more examples!

# Contributing

For problems with this library, please open a GitHub issue or a pull-request. 

For issues with Fenrir Cloud, please contact customer support or your account manager.

# License

Fenrir.Multiplayer is an open source software, licensed under the terms of MIT license. See [LICENSE.txt](/LICENSE.txt) for details.
