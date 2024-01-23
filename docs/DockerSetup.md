# Docker Setup

This section explains how to run Fenrir Server using Docker

Previous Section: [Logging](/Logging.md)

## Building Server solution with Docker

Fenrir Server is a standalone .NET application that supports Docker out of the box. 

When using built-in template, you can navigate to the `Server` folder and build it using provided dockerfile:

```bash
cd Server
docker build . -t example:latest 
```

**Note**: When running docker, make sure to bind both **TCP** and **UDP** ports:

```bash
docker run -p 27016:27016/tcp -p 27016:27016/udp example:latest
```


Next Section: [Thread Safety](/ThreadSafety.md)
