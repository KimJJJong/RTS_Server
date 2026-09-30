[한국어](README.md) | [日本語](README.ja.md) | [English](README.en.md)

# WAR'LD Painting Game Server

This is a **.NET 8-based game server** that supports WAR'LD Painting's 1v1 real-time PvP. It separates the ASP.NET Core Match API from the C# TCP GameServer and implements `SocketAsyncEventArgs`-based asynchronous sessions, 2-byte length-prefix framing, result Tick packets, and a JSON log pipeline.

## Tech Stack

- **Language / Runtime**: C#, .NET 8
- **API**: ASP.NET Core Match API
- **Real-time communication**: TCP/IP, `SocketAsyncEventArgs`
- **Packets**: 2-byte length-prefix framing, XML PDL-based code generation
- **Test client**: .NET 8 Console DummyClient
- **Logging**: JSON log, Filebeat, Elasticsearch, Kibana
- **Game client**: Unity (the team's repository is private)

> The `Server`, `HttpApi`, `DummyClient`, `ServerCore`, `Shared`, and `PacketGenerator` projects in this repository all target `net8.0`.

## Architecture

```text
Client
  ├─ HTTP match request/status ──> ASP.NET Core Match API
  └─ TCP room/gameplay packets ──> C# TCP GameServer

PacketGenerator ── PDL.xml ──> packet classes / dispatchers
GameServer ── JSON log ──> Filebeat ──> Elasticsearch ──> Kibana
```

- `HttpApi`: Match requests, cancellations, status queries, and GameServer Room creation requests
- `Server`: TCP sessions, Room admission, combat request validation, and result packet scheduling based on a 33 ms logical Tick
- `ServerCore`: Listener, session, receive/send buffer, and send queue
- `PacketGenerator`: Packet class and manager generation based on `PDL.xml`
- `DummyClient`: .NET 8 client for server communication verification
- `Shared`: Shared contracts and JSON logging

## Synchronization Scope

The server processes selected result states, including the OID pool, coordinate conversion, HP, capture progress, and win/loss state, while clients use the received reference values for presentation. Here, 33 ms does not mean that the entire Room is simulated at 30 Hz; it is the logical Tick interval used to align when result packets are applied.

## Build

```bash
dotnet build Server.sln
```

The Unity Client repository is private because it is a team asset. The public portfolio uses only minimal code excerpts that explain the network processing and Result Tick scheduling.
