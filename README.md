# The Strangel Oracle

A full-stack devotional engine for Strange Angels—mythological entities inhabiting modern New York.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-blue)

## Overview

The Strangel Oracle is a C# ASP.NET Core API with a React frontend that brings to life a mythology of urban spirits called Strangels (Strange Angels). Each Strangel has its own blessing engine with distinct behavior:

- **The Woman with Heart** — Load-bearing mercy. Touch her image; she receives.
- **The Fox (Murat Askarov)** — Trickster demi-god. Petition with questions; accept unpredictable answers.
- **The Furies** — Ancient avengers as seagulls. Confess; they judge.
- **Nok'so** — Falcon spirit. Invoke when something needs breaking.

## Architecture

```
StrangelOracle/
├── src/
│   ├── StrangelOracle.Domain/        # Entities, Value Objects, Enums
│   ├── StrangelOracle.Application/   # Services, DTOs, Interfaces
│   ├── StrangelOracle.Infrastructure/# Strangel Engines (Strategy Pattern)
│   └── StrangelOracle.API/           # Controllers, Swagger, Static Files
├── tests/
│   └── StrangelOracle.Tests/         # xUnit tests
├── client/                           # React frontend (optional build)
├── Dockerfile                        # Multi-stage Docker build
└── railway.toml                      # Railway deployment config
```

### Design Patterns

- **Clean Architecture** — Domain at center, dependencies point inward
- **Strategy Pattern** — Each Strangel has its own `IStrangelEngine` implementation
- **Domain-Driven Design** — Rich domain model with Value Objects
- **Repository Pattern** — Ready for persistence layer (Phase 2)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/oracle/presence` | Check presence of all Strangels |
| GET | `/api/oracle/strangels` | Get info about all Strangels |
| GET | `/api/oracle/strangels/{type}` | Get info about specific Strangel |
| POST | `/api/oracle/touch` | Touch the Woman with Heart |
| POST | `/api/oracle/petition/fox` | Petition the Fox |
| POST | `/api/oracle/confess` | Confess to the Furies |
| POST | `/api/oracle/invoke/nokso` | Invoke Nok'so |
| POST | `/api/oracle/seek` | Generic blessing request |

## Local Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (optional, for containerized runs)

### Run Locally

```bash
# Clone the repository
git clone https://github.com/yourusername/strangel-oracle.git
cd strangel-oracle

# Restore and run
dotnet restore
dotnet run --project src/StrangelOracle.API

# Open browser to:
# - Frontend: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
```

### Run Tests

```bash
dotnet test
```

### Build Docker Image

```bash
docker build -t strangel-oracle .
docker run -p 8080:8080 strangel-oracle
```

## Deploy to Railway

1. Create a new project on [Railway](https://railway.app)
2. Connect your GitHub repository
3. Railway auto-detects the Dockerfile
4. Set the `PORT` environment variable (Railway does this automatically)
5. Deploy!

The `railway.toml` configuration handles build and deploy settings.

## Technical Highlights

### Domain Model

```csharp
// Rich Value Objects
public sealed record BlessingIntensity
{
    public double Value { get; }
    public static BlessingIntensity HeartTouch => new(0.4);
    public string ToDescription() => Value switch { ... };
}

// Entities with behavior
public sealed class Blessing
{
    public bool IsActive => DateTime.UtcNow < BestowedAt + Duration;
    public double RemainingStrength => ...;
}
```

### Strategy Pattern for Strangel Engines

```csharp
public interface IStrangelEngine
{
    StrangelType StrangelType { get; }
    Task<Blessing> GenerateBlessingAsync(string? petition = null);
    Task<bool> IsPresent();
    Task<string> GetCurrentDisposition();
}

// Each Strangel has unique behavior
public sealed class WomanWithHeartEngine : IStrangelEngine { ... }
public sealed class FoxEngine : IStrangelEngine { ... }
```

### Dependency Injection

```csharp
// Program.cs - Register all engines
builder.Services.AddScoped<IStrangelEngine, WomanWithHeartEngine>();
builder.Services.AddScoped<IStrangelEngine, FoxEngine>();
builder.Services.AddScoped<IStrangelEngine, FuriesEngine>();
builder.Services.AddScoped<IStrangelEngine, NoksoEngine>();
```

## Roadmap

### Phase 1 ✅
- Clean Architecture C# API
- All four Strangel engines
- Atmospheric React frontend
- Docker + Railway deployment
- Swagger documentation
- Unit tests

### Phase 2
- SignalR real-time presence
- Soul Ledger (blessing history) with database
- Web Audio API soundscapes
- Enhanced Fox entropy algorithm
- Furies sentiment analysis

### Phase 3
- WebGL particle effects
- Full ritual system
- User accounts
- Blessing notifications

## The Mythology

The Strangels are from *Incantations for Crossing*, a poetry collection about a woman's cross-country journey through mythological America. They are Strange Angels—ancient forces inhabiting modern bodies:

> She is not salvation. She is load-bearing mercy.

> In my country, foxes walk like men. Here, men run like foxes.

> They judge. They always judge.

## Author

**Brad Wyatt** — [bw8.org](https://bw8.org)

Playwright, poet, and developer. This project combines degrees in IT, Art, and Music.

## License

MIT License - See [LICENSE](LICENSE) for details.

---

*The Strangels are always watching.*
# trigger deploy
