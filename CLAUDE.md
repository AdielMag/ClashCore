# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a multiplayer game project called "MyApp" with a Unity client and .NET backend services. The architecture follows a microservices pattern with real-time communication using MagicOnion (gRPC-based framework).

### Key Components

1. **Unity Client** (`src/MyApp.Unity/`) - Unity-based game client with modular architecture
2. **Backend Services** (`src/MyApp.Server.Services/`) - gRPC API services for matchmaking and player management
3. **GameHub** (`src/MyApp.Server.GameHub/`) - Real-time game hub using MagicOnion for multiplayer sessions
4. **Shared Code** (`src/MyApp.Shared/`) - Common data models and interfaces shared between client and server

## Development Commands

### Building the Project

**Server Projects (from repository root):**
```bash
# Build all server projects
dotnet build MyApp.sln

# Build specific projects
dotnet build src/MyApp.Server.Services/MyApp.Server.Services.csproj
dotnet build src/MyApp.Server.GameHub/MyApp.Server.GameHub.csproj
```

**Unity Project:**
- Open `src/MyApp.Unity/` in Unity Editor
- Use Unity's standard build process (File > Build Settings)

### Running Services

**Using Docker Compose (Recommended):**
```bash
docker-compose up
```
This starts:
- MongoDB with replica set on port 27017
- API Services on port 5002
- GameHub on port 12346

**Manual Development:**
```bash
# Run API Services
dotnet run --project src/MyApp.Server.Services/MyApp.Server.Services.csproj

# Run GameHub
dotnet run --project src/MyApp.Server.GameHub/MyApp.Server.GameHub.csproj
```

### Testing

```bash
# Run tests for specific projects (if test projects exist)
dotnet test src/MyApp.Unity/App.Tests.csproj
```

## Architecture Details

### Unity Client Structure

The Unity project uses a modular domain-driven architecture with VContainer for dependency injection:

- **App Domain** (`Assets/App/`) - Application bootstrap and lifecycle management
- **Game Domain** (`Assets/App/SubDomains/Game/`) - Core gameplay systems
- **Lobby Domain** (`Assets/App/SubDomains/Lobby/`) - Lobby and matchmaking UI

Key Unity Subdomain Modules:
- `CameraManager` - Camera control and management
- `Environment` - Game world and environment systems
- `PlayersManager` - Player spawning, movement, and state management
- `GameNetworkHub` - Network communication with backend
- `InputManager` - Input handling and controls
- `ProximityService` - Spatial awareness and proximity calculations

### Backend Services

**MyApp.Server.Services** - Main API services:
- `PlayersService` - Player management and authentication
- `MatchMakerService` - Matchmaking and game session creation
- `MatchInstanceService` - Game instance lifecycle management

**MyApp.Server.GameHub** - Real-time game communication:
- Uses MagicOnion for real-time bidirectional communication
- Handles player movement, game state synchronization
- Manages room-based multiplayer sessions

### Database

- MongoDB with replica set configuration
- Collections: Players, Matches, MatchInstances, Configs
- Connection string uses authentication: `mongodb://root:example@mongo:27017/solaria?authSource=admin&replicaSet=rs0`

### Technology Stack

- **.NET 8.0** - Backend services
- **Unity 2022.3+** - Game client
- **MagicOnion 7.0** - Real-time communication framework
- **MessagePack** - Serialization
- **MongoDB 7.0** - Database
- **VContainer** - Unity dependency injection
- **UniTask** - Unity async/await support

## Development Notes

### Unity Assembly Definitions

The Unity project uses assembly definitions for modular compilation:
- Each subdomain has its own `.asmdef` file
- Test assemblies are separated (e.g., `App.Tests.asmdef`)
- This enables faster compilation and clearer dependencies

### MagicOnion Integration

- Shared interfaces in `MyApp.Shared/` define service contracts
- Client-side proxies are auto-generated for Unity
- Real-time hubs use `IGameHub` and `IGameHubReceiver` interfaces

### Docker Development

The project includes multi-stage Dockerfiles:
- `src/Dockerfile` builds both Services and GameHub images
- Development environment uses `docker-compose.yml`
- HTTPS certificates are auto-generated for development

### MongoDB Setup

MongoDB requires replica set initialization for proper operation. The docker-compose setup handles this automatically with health checks and initialization scripts.