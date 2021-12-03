# Installation

## Installing using Unity Package Manager

This package can be installed using Unity Package from `https://upm.fenrirserver.org` registry.

![Fenrir Multiplayer](/docs/images/UnityPackageManager.png)

1. In Unity, open **Edit** → **Project Settings** → **Package Manager** and add a **Scoped Registry** using URL: `https://upm.fenrirserver.org`
2. Open **Window** → **Package Manager** and switch to **Packages: My Registries**. Select **Fenrir Multiplayer** and click **Install**

## Installing Using NuGet Directly

Note: Unity Package Manager package comes with a server template that has a NuGet package installed (explained in the next section)

Unless you are creating a server project from scratch, **you do not need to install NuGet package directly**. 

To install or update the NuGet package, you can use **Project** → **Manage NuGet Packages** and search for **Fenrir.Multiplayer** package.

You can also just run:

```bash
dotnet add package Fenrir.Multiplayer 
```

Next Section: [Quick Start](QuickStart.md)
