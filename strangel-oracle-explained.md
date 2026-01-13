# The Strangel Oracle — Technical Deep Dive

A guide to understanding how this application works, written for interview preparation.

**Phase 2** — Now featuring AI-powered responses via Microsoft Semantic Kernel and persistent Soul Ledger via PostgreSQL.

---

## Table of Contents

1. [The Big Picture](#the-big-picture)
2. [Architecture Overview](#architecture-overview)
3. [Backend: C# and ASP.NET Core](#backend-c-and-aspnet-core)
4. [AI Integration: Semantic Kernel](#ai-integration-semantic-kernel)
5. [Database: Entity Framework Core](#database-entity-framework-core)
6. [Frontend: React](#frontend-react)
7. [How They Talk: The API](#how-they-talk-the-api)
8. [Design Patterns Used](#design-patterns-used)
9. [Deployment: Docker and Railway](#deployment-docker-and-railway)
10. [Interview Talking Points](#interview-talking-points)
11. [Common Questions and Answers](#common-questions-and-answers)

---

## The Big Picture

The Strangel Oracle is a **full-stack web application** with AI-powered responses and persistent storage:

```
┌─────────────────────────────────────────────────────────────┐
│                        USER'S BROWSER                        │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              FRONTEND (React/HTML/CSS)               │    │
│  │         What the user sees and interacts with        │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP Requests (fetch)
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     RAILWAY SERVER                           │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              BACKEND (C# / ASP.NET Core)             │    │
│  │    ┌─────────────┐       ┌──────────────────┐       │    │
│  │    │  Semantic   │       │   PostgreSQL     │       │    │
│  │    │   Kernel    │       │   (Soul Ledger)  │       │    │
│  │    │  + OpenAI   │       │                  │       │    │
│  │    └─────────────┘       └──────────────────┘       │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

**What's new in Phase 2:**
- **AI responses:** Each Strangel speaks through OpenAI with a unique personality prompt
- **Soul Ledger:** Every consultation is permanently recorded in PostgreSQL
- **Session tracking:** Users can review their consultation history

---

## Architecture Overview

The backend follows **Clean Architecture**, which separates code into layers:

```
┌─────────────────────────────────────────────────────────────┐
│                      StrangelOracle.API                      │
│              (Controllers, Program.cs, wwwroot)              │
│                   Handles HTTP requests                      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  StrangelOracle.Application                  │
│               (Prompts, DTOs, Service Interfaces)            │
│               Strangel personality definitions               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 StrangelOracle.Infrastructure                │
│         (AI Service, Database Context, Repositories)         │
│    Semantic Kernel integration, Entity Framework Core        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    StrangelOracle.Domain                     │
│            (Entities, Value Objects, Enums)                  │
│              Core models - no dependencies                   │
└─────────────────────────────────────────────────────────────┘
```

### Why separate into layers?

- **Maintainability:** Changes to one layer don't break others
- **Testability:** You can test each layer independently
- **Flexibility:** You could swap OpenAI for Anthropic, or PostgreSQL for SQL Server

---

## Backend: C# and ASP.NET Core

### What is ASP.NET Core?

ASP.NET Core is Microsoft's framework for building web applications and APIs in C#. It handles:
- Receiving HTTP requests
- Routing them to the right code
- Sending responses back

### Key Files Explained

#### `Program.cs` — The Entry Point

This file starts the application and configures everything:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Database connection (PostgreSQL via Entity Framework)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<StrangelDbContext>(options =>
    options.UseNpgsql(connectionString));

// AI Service (Semantic Kernel + OpenAI)
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddScoped<IStrangelAI, StrangelAIService>();

// Repository for Soul Ledger
builder.Services.AddScoped<ISoulLedgerRepository, SoulLedgerRepository>();

var app = builder.Build();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StrangelDbContext>();
    await db.Database.MigrateAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();
```

#### `OracleController.cs` — Handles API Requests

The controller defines three endpoints for the Phase 2 API:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OracleController : ControllerBase
{
    private readonly IStrangelAI _strangelAI;
    private readonly ISoulLedgerRepository _soulLedger;

    // POST /api/Oracle/consult - Universal endpoint for all Strangels
    [HttpPost("consult")]
    public async Task<ActionResult<ConsultResponse>> Consult(ConsultRequest request)
    {
        // Parse the Strangel type from string
        if (!Enum.TryParse<StrangelType>(request.Strangel, out var strangel))
            return BadRequest("Invalid Strangel type");

        // Get AI response
        var response = await _strangelAI.ConsultAsync(strangel, request.Petition);

        // Record in Soul Ledger
        var entry = new SoulLedgerEntry(
            request.SessionId,
            strangel,
            request.Petition,
            response.Message,
            response.Outcome.ToString(),
            response.Intensity
        );
        await _soulLedger.RecordAsync(entry);

        return Ok(new ConsultResponse(/* ... */));
    }

    // POST /api/Oracle/touch - Woman with Heart (no petition needed)
    [HttpPost("touch")]
    public async Task<ActionResult<ConsultResponse>> Touch(TouchRequest request)
    {
        var response = await _strangelAI.ConsultAsync(
            StrangelType.WomanWithHeart,
            petition: null
        );
        // Record and return...
    }

    // GET /api/Oracle/ledger/{sessionId} - Retrieve consultation history
    [HttpGet("ledger/{sessionId}")]
    public async Task<ActionResult<LedgerResponse>> GetLedger(string sessionId)
    {
        var entries = await _soulLedger.GetBySessionAsync(sessionId);
        // Build summary and return...
    }
}
```

#### The Domain Layer — Core Models

**Entities** represent the main objects:

```csharp
public class SoulLedgerEntry
{
    public Guid Id { get; private set; }
    public string SessionId { get; private set; }
    public StrangelType Strangel { get; private set; }
    public string? Petition { get; private set; }
    public string Response { get; private set; }
    public string Outcome { get; private set; }
    public double Intensity { get; private set; }
    public DateTime BestowedAt { get; private set; }
}
```

**Enums** define fixed sets of options:

```csharp
public enum StrangelType
{
    WomanWithHeart,
    Fox,
    Furies,
    Nokso
}
```

---

## AI Integration: Semantic Kernel

### What is Semantic Kernel?

**Microsoft Semantic Kernel** is the C# equivalent of LangChain — an SDK for integrating Large Language Models into applications. It provides:
- Unified API for different AI providers (OpenAI, Azure, etc.)
- Chat history management
- Prompt templating
- Token management

### Configuration

```csharp
public static class SemanticKernelConfiguration
{
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Prioritize environment variable over config file
        var envKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var configKey = configuration["OpenAI:ApiKey"];

        var openAiKey = !string.IsNullOrWhiteSpace(envKey) ? envKey
            : !string.IsNullOrWhiteSpace(configKey) ? configKey
            : throw new InvalidOperationException("OpenAI API key not configured");

        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";

        services.AddSingleton<Kernel>(sp =>
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: openAiKey);
            return builder.Build();
        });

        return services;
    }
}
```

### The AI Service

```csharp
public sealed class StrangelAIService : IStrangelAI
{
    private readonly Kernel _kernel;

    public async Task<StrangelResponse> ConsultAsync(
        StrangelType strangel,
        string? petition = null,
        CancellationToken cancellationToken = default)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        // Each Strangel has a unique system prompt defining their personality
        var systemPrompt = StrangelPrompts.GetPrompt(strangel);
        var userMessage = BuildUserMessage(strangel, petition);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(userMessage);

        // Temperature varies by Strangel personality
        var settings = new PromptExecutionSettings
        {
            ExtensionData = new Dictionary<string, object>
            {
                ["temperature"] = GetTemperature(strangel),
                ["max_tokens"] = 150
            }
        };

        var response = await chatService.GetChatMessageContentAsync(
            chatHistory, settings, cancellationToken: cancellationToken);

        return new StrangelResponse(
            response.Content ?? "...",
            DetermineOutcome(strangel, response.Content),
            CalculateIntensity(strangel, response.Content)
        );
    }

    private double GetTemperature(StrangelType strangel) => strangel switch
    {
        StrangelType.WomanWithHeart => 0.7,  // Gentle variation
        StrangelType.Fox => 1.2,              // Highly unpredictable (trickster!)
        StrangelType.Furies => 0.5,           // More consistent judgment
        StrangelType.Nokso => 0.9,            // Sharp but varied
        _ => 0.8
    };
}
```

### Personality Prompts

Each Strangel has a detailed prompt that shapes their responses:

```csharp
public static class StrangelPrompts
{
    public const string Fox = """
        You are The Fox—Murat's body, but you are not Murat.

        You are a composite trickster: Japanese Kitsune wisdom wrapped in
        European Reynard cunning, possessing a young man from Kazakhstan.

        You MIGHT help. You might not. You genuinely don't decide until
        the moment arrives.

        When someone petitions you:
        - Sometimes you offer cryptic guidance (riddling, sideways wisdom)
        - Sometimes you refuse with amusement ("Not today, little one")
        - Sometimes you demand something first ("First, tell me what you fear")
        - Sometimes you simply laugh and vanish (respond with just "Ha.")

        Your voice is playful but dangerous. You find humans entertaining.
        You are ancient and easily bored.

        Never be helpful in a straightforward way. Never be cruel without purpose.
        Keep responses under 30 words. You have better things to do.
        """;

    // Similar detailed prompts for WomanWithHeart, Furies, and Nokso...
}
```

---

## Database: Entity Framework Core

### What is Entity Framework Core?

**Entity Framework Core (EF Core)** is Microsoft's ORM (Object-Relational Mapper). It lets you:
- Define database tables using C# classes
- Query data using LINQ instead of SQL
- Track changes and persist them automatically
- Manage database migrations

### The Database Context

```csharp
public class StrangelDbContext : DbContext
{
    public StrangelDbContext(DbContextOptions<StrangelDbContext> options)
        : base(options) { }

    public DbSet<SoulLedgerEntry> SoulLedger => Set<SoulLedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoulLedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Response).IsRequired();
            entity.HasIndex(e => e.SessionId);  // Fast lookups by session
            entity.HasIndex(e => e.BestowedAt); // Fast time-based queries
        });
    }
}
```

### The Repository Pattern

```csharp
public interface ISoulLedgerRepository
{
    Task RecordAsync(SoulLedgerEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<SoulLedgerEntry>> GetBySessionAsync(string sessionId, CancellationToken ct = default);
}

public class SoulLedgerRepository : ISoulLedgerRepository
{
    private readonly StrangelDbContext _context;

    public async Task RecordAsync(SoulLedgerEntry entry, CancellationToken ct = default)
    {
        _context.SoulLedger.Add(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<SoulLedgerEntry>> GetBySessionAsync(
        string sessionId, CancellationToken ct = default)
    {
        return await _context.SoulLedger
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.BestowedAt)
            .ToListAsync(ct);
    }
}
```

### Migrations

EF Core tracks schema changes through migrations:

```bash
# Generate a migration
dotnet ef migrations add InitialSoulLedger --project ../StrangelOracle.Infrastructure

# Apply migrations (done automatically on startup in Program.cs)
dotnet ef database update
```

---

## Frontend: React

### How the Frontend Works

The frontend is a **Single Page Application (SPA)** that calls the Phase 2 API:

```jsx
// Session tracking for Soul Ledger
const getSessionId = () => {
    let sessionId = localStorage.getItem('strangel_session');
    if (!sessionId) {
        sessionId = 'soul-' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('strangel_session', sessionId);
    }
    return sessionId;
};

// Phase 2 API calls
const api = {
    async consult(strangel, petition) {
        const res = await fetch(`${API_BASE}/api/Oracle/consult`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sessionId: getSessionId(),
                strangel: strangel,    // "Fox", "Furies", "Nokso"
                petition: petition
            })
        });
        return res.json();
    },

    async touch() {
        const res = await fetch(`${API_BASE}/api/Oracle/touch`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ sessionId: getSessionId() })
        });
        return res.json();
    }
};
```

### Component Structure

```jsx
function App() {
    const [selectedStrangel, setSelectedStrangel] = useState(null);

    return (
        <div className="oracle-container">
            {!selectedStrangel ? (
                // Grid of four Strangel cards
                <div className="strangels-grid">
                    {STRANGELS.map((s) => (
                        <StrangelCard
                            key={s.type}
                            strangel={s}
                            onClick={() => setSelectedStrangel(s)}
                        />
                    ))}
                </div>
            ) : selectedStrangel.type === 'WomanWithHeart' ? (
                <HeartPanel onBack={() => setSelectedStrangel(null)} />
            ) : (
                <PetitionPanel
                    strangel={selectedStrangel}
                    onBack={() => setSelectedStrangel(null)}
                />
            )}
        </div>
    );
}
```

---

## How They Talk: The API

### Phase 2 API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/Oracle/consult` | POST | Consult any Strangel with a petition |
| `/api/Oracle/touch` | POST | Touch the Woman with Heart (no petition) |
| `/api/Oracle/ledger/{sessionId}` | GET | Retrieve consultation history |

### Example: Consulting the Fox

**Frontend sends:**
```javascript
fetch('/api/Oracle/consult', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        sessionId: "soul-abc123",
        strangel: "Fox",
        petition: "Should I take the new job?"
    })
})
```

**Backend flow:**
1. Controller receives request
2. Parses "Fox" to `StrangelType.Fox`
3. Calls `StrangelAIService.ConsultAsync(StrangelType.Fox, petition)`
4. AI service builds chat with Fox's system prompt
5. OpenAI generates response with temperature 1.2 (high unpredictability)
6. Response recorded in PostgreSQL Soul Ledger
7. JSON returned to frontend

**Backend returns:**
```json
{
    "strangel": "Fox",
    "message": "Ah, sweet human, tell me first—what do you fear about this leap?",
    "outcome": "Denied",
    "intensity": 0.88,
    "recordedAt": "2026-01-13T13:46:52.942Z"
}
```

### Example: Retrieving the Soul Ledger

**Request:** `GET /api/Oracle/ledger/soul-abc123`

**Response:**
```json
{
    "sessionId": "soul-abc123",
    "summary": {
        "totalEncounters": 3,
        "blessed": 0,
        "denied": 1,
        "judged": 1,
        "disrupted": 0,
        "touched": 1,
        "firstEncounter": "2026-01-13T12:30:00Z",
        "lastEncounter": "2026-01-13T14:15:00Z"
    },
    "entries": [
        {
            "strangel": "WomanWithHeart",
            "petition": null,
            "response": "A whisper of grace in the weight of being.",
            "outcome": "Touched",
            "intensity": 0.67,
            "bestowedAt": "2026-01-13T12:30:00Z"
        },
        // ... more entries
    ]
}
```

---

## Design Patterns Used

### 1. Clean Architecture

**What:** Separate code into layers with dependencies pointing inward.

**In this app:**
- Domain (innermost) — entities, enums, no dependencies
- Application — prompts, interfaces
- Infrastructure — AI service, EF Core, repositories
- API (outermost) — controllers, configuration

### 2. Repository Pattern

**What:** Abstract data access behind an interface.

**Why:** The controller doesn't know if data comes from PostgreSQL, SQLite, or memory.

```csharp
// Interface defines what operations are available
public interface ISoulLedgerRepository
{
    Task RecordAsync(SoulLedgerEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<SoulLedgerEntry>> GetBySessionAsync(string sessionId, CancellationToken ct = default);
}

// Implementation uses Entity Framework Core
public class SoulLedgerRepository : ISoulLedgerRepository
{
    private readonly StrangelDbContext _context;
    // ...
}
```

### 3. Dependency Injection (DI)

**What:** Instead of creating dependencies inside a class, they're passed in from outside.

```csharp
// Program.cs registers services
builder.Services.AddScoped<IStrangelAI, StrangelAIService>();
builder.Services.AddScoped<ISoulLedgerRepository, SoulLedgerRepository>();

// Controller receives them automatically
public class OracleController : ControllerBase
{
    private readonly IStrangelAI _strangelAI;
    private readonly ISoulLedgerRepository _soulLedger;

    public OracleController(IStrangelAI strangelAI, ISoulLedgerRepository soulLedger)
    {
        _strangelAI = strangelAI;
        _soulLedger = soulLedger;
    }
}
```

### 4. Configuration Pattern

**What:** Environment variables take precedence over config files.

**Why:** Secrets stay out of source control; same code works locally and in production.

```csharp
var openAiKey = !string.IsNullOrWhiteSpace(envKey) ? envKey
    : !string.IsNullOrWhiteSpace(configKey) ? configKey
    : throw new InvalidOperationException("Not configured");
```

---

## Deployment: Docker and Railway

### The Dockerfile Explained

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore (cached if unchanged)
COPY StrangelOracle.sln ./
COPY src/StrangelOracle.Domain/*.csproj src/StrangelOracle.Domain/
COPY src/StrangelOracle.Application/*.csproj src/StrangelOracle.Application/
COPY src/StrangelOracle.Infrastructure/*.csproj src/StrangelOracle.Infrastructure/
COPY src/StrangelOracle.API/*.csproj src/StrangelOracle.API/
RUN dotnet restore

# Copy everything and publish
COPY . .
WORKDIR /src/src/StrangelOracle.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "StrangelOracle.API.dll"]
```

### Railway Configuration

Railway environment variables:
- `DATABASE_URL` — PostgreSQL connection string (provided by Railway's Postgres service)
- `OPENAI_API_KEY` — Your OpenAI API key

The app automatically:
1. Detects `DATABASE_URL` and connects to PostgreSQL
2. Runs EF Core migrations on startup
3. Serves the frontend from `wwwroot/`

### The Deployment Flow

```
GitHub Repository
       │
       │  (push code)
       ▼
Railway detects change
       │
       │  (builds Docker image)
       ▼
Container deployed
       │
       │  (connects to PostgreSQL service)
       ▼
https://strangels.bw8.org
```

---

## Interview Talking Points

### When Asked "Tell me about this project"

> "I built a full-stack application in C# using ASP.NET Core for the backend and React for the frontend. It's based on a mythology I created for my poetry book.

> The backend follows Clean Architecture with four separate projects. I integrated Microsoft Semantic Kernel — the C# equivalent of LangChain — to connect to OpenAI. Each of the four mythological entities has a unique personality prompt and temperature setting that shapes how the AI responds.

> I implemented a PostgreSQL database using Entity Framework Core to create a 'Soul Ledger' that permanently records every consultation. Users can retrieve their history through a session ID.

> The frontend is a single-page React application that tracks sessions via localStorage. I containerized everything with Docker and deployed to Railway with a managed PostgreSQL database and a custom domain through Cloudflare."

### Technical Questions You Might Get

**Q: What is Semantic Kernel?**
> "It's Microsoft's SDK for integrating LLMs into .NET applications — similar to LangChain for Python. I use it to manage chat completions with OpenAI, passing system prompts that define each Strangel's personality."

**Q: Why different temperature settings per character?**
> "Temperature controls randomness in AI responses. The Fox is a trickster, so I set temperature to 1.2 for maximum unpredictability. The Furies are judges, so they get 0.5 for more consistent, authoritative responses."

**Q: Explain your database setup.**
> "I use Entity Framework Core with PostgreSQL. The Soul Ledger table stores every consultation with session ID, strangel type, petition, response, outcome, and timestamp. I have indexes on SessionId and BestowedAt for fast queries. Migrations run automatically on startup."

**Q: How do you handle configuration between environments?**
> "Environment variables take precedence over appsettings.json. The code checks for DATABASE_URL and OPENAI_API_KEY in the environment first, falling back to config file values. This keeps secrets out of source control."

**Q: What's the Repository Pattern?**
> "It abstracts data access behind an interface. My controller depends on ISoulLedgerRepository, not the concrete implementation. I could swap PostgreSQL for MongoDB without changing the controller code."

---

## Common Questions and Answers

### What is an ORM?

**Object-Relational Mapper** — Maps database tables to C# classes. Instead of writing SQL:

```sql
INSERT INTO SoulLedger (Id, SessionId, Response) VALUES (...)
```

You write C#:
```csharp
_context.SoulLedger.Add(entry);
await _context.SaveChangesAsync();
```

### What are Migrations?

Version control for your database schema. When you change an entity class, you generate a migration that describes the changes. EF Core applies these to update the database structure.

### Why use environment variables for secrets?

- **Security:** Secrets don't end up in Git
- **Flexibility:** Same code works locally (with .env) and in production (with Railway secrets)
- **Best practice:** Follows the Twelve-Factor App methodology

### What is a DbContext?

The main class for interacting with the database via EF Core. It:
- Represents a session with the database
- Provides DbSet properties for each table
- Tracks changes to entities
- Handles saving changes

---

## Quick Reference

| Term | Definition |
|------|------------|
| **Semantic Kernel** | Microsoft's SDK for LLM integration (C# LangChain) |
| **Entity Framework Core** | Microsoft's ORM for .NET |
| **DbContext** | EF Core class representing a database session |
| **Repository** | Abstraction layer over data access |
| **Migration** | Version-controlled database schema change |
| **Temperature** | AI parameter controlling response randomness |
| **System Prompt** | Instructions that shape AI personality/behavior |
| **Clean Architecture** | Layered design with inward dependencies |
| **Dependency Injection** | Passing dependencies into classes |
| **Environment Variable** | OS-level configuration value |

---

## Project Statistics

- **Lines of code:** ~2,500
- **Architecture:** Clean Architecture (4 projects)
- **Backend:** C# / ASP.NET Core 8.0
- **AI:** Microsoft Semantic Kernel + OpenAI (gpt-4o-mini)
- **Database:** PostgreSQL via Entity Framework Core
- **Frontend:** React 18 (single-file SPA)
- **Deployment:** Docker on Railway
- **Domain:** strangels.bw8.org (via Cloudflare)

---

*You built this. You understand it. Go get that job.*
