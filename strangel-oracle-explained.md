# The Strangel Oracle — Technical Deep Dive

A guide to understanding how this application works, written for interview preparation.

---

## Table of Contents

1. [The Big Picture](#the-big-picture)
2. [Architecture Overview](#architecture-overview)
3. [Backend: C# and ASP.NET Core](#backend-c-and-aspnet-core)
4. [Frontend: React](#frontend-react)
5. [How They Talk: The API](#how-they-talk-the-api)
6. [Design Patterns Used](#design-patterns-used)
7. [Deployment: Docker and Railway](#deployment-docker-and-railway)
8. [Interview Talking Points](#interview-talking-points)
9. [Common Questions and Answers](#common-questions-and-answers)

---

## The Big Picture

The Strangel Oracle is a **full-stack web application** with two main parts:

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
│  │         Processes requests, generates blessings      │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

**Simple explanation:** The user's browser loads a webpage (frontend). When they click a button like "Touch" or "Invoke," the browser sends a request to a server (backend). The server processes it and sends back data. The browser displays that data.

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
│                  (Services, DTOs, Interfaces)                │
│                   Business logic lives here                  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 StrangelOracle.Infrastructure                │
│              (Strangel Engines - the actual logic)           │
│         WomanWithHeartEngine, FoxEngine, etc.               │
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
- **Flexibility:** You could swap out the database or UI without rewriting everything

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

// Register services (Dependency Injection)
builder.Services.AddScoped<IStrangelEngine, WomanWithHeartEngine>();
builder.Services.AddScoped<IStrangelEngine, FoxEngine>();
// ... etc

var app = builder.Build();

// Configure middleware (order matters!)
app.UseDefaultFiles();  // Serves index.html for root URL
app.UseStaticFiles();   // Serves files from wwwroot folder
app.MapControllers();   // Routes API requests to controllers

app.Run();  // Start listening for requests
```

#### `OracleController.cs` — Handles API Requests

Controllers define the API endpoints. Each method handles a specific URL:

```csharp
[ApiController]
[Route("api/[controller]")]  // Base URL: /api/oracle
public class OracleController : ControllerBase
{
    // GET /api/oracle/strangels
    [HttpGet("strangels")]
    public async Task<ActionResult<IEnumerable<StrangelInfo>>> GetAllStrangels()
    {
        var strangels = await _oracleService.GetAllStrangelsAsync();
        return Ok(strangels);  // Returns JSON
    }
    
    // POST /api/oracle/touch
    [HttpPost("touch")]
    public async Task<ActionResult<BlessingResponse>> TouchHeart()
    {
        var blessing = await _oracleService.TouchHeartAsync();
        return Ok(blessing);  // Returns JSON
    }
}
```

**Key concept:** `[HttpGet]` and `[HttpPost]` are **attributes** that tell ASP.NET which HTTP method and URL this code handles.

#### The Domain Layer — Core Models

**Entities** represent the main objects:

```csharp
public class Blessing
{
    public Guid Id { get; private set; }
    public StrangelType Source { get; private set; }
    public string Message { get; private set; }
    public BlessingIntensity Intensity { get; private set; }
    
    // Computed property - calculates if blessing is still active
    public bool IsActive => DateTime.UtcNow < BestowedAt + Duration;
}
```

**Value Objects** are immutable objects defined by their values:

```csharp
public sealed record BlessingIntensity
{
    public double Value { get; }
    
    // Factory methods for common intensities
    public static BlessingIntensity Gentle => new(0.3);
    public static BlessingIntensity Strong => new(0.7);
    
    // Converts the number to human-readable text
    public string ToDescription() => Value switch
    {
        < 0.2 => "barely perceptible",
        < 0.4 => "gentle, like a held breath",
        < 0.6 => "present and undeniable",
        _ => "overwhelming"
    };
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

#### The Strangel Engines — Where Blessings Come From

Each Strangel has its own "engine" that generates blessings:

```csharp
public class WomanWithHeartEngine : IStrangelEngine
{
    public StrangelType StrangelType => StrangelType.WomanWithHeart;
    
    public Task<Blessing> GenerateBlessingAsync(string? petition = null)
    {
        // She ignores petitions - she blesses because she must
        var message = BlessingMessages[random.Next(BlessingMessages.Length)];
        
        return Task.FromResult(Blessing.Create(
            source: StrangelType.WomanWithHeart,
            type: BlessingType.Blessing,
            intensity: BlessingIntensity.HeartTouch,
            message: message
        ));
    }
}
```

**Why separate engines?** Each Strangel behaves differently:
- Woman with Heart always blesses
- Fox sometimes refuses or tricks
- Furies always judge
- Nok'so disrupts or protects

This is the **Strategy Pattern** — same interface, different behaviors.

---

## Frontend: React

### What is React?

React is a JavaScript library for building user interfaces. It lets you create **components** — reusable pieces of UI.

### How the Frontend Works

The frontend is a **Single Page Application (SPA)**. One HTML file (`index.html`) contains:
1. CSS styles
2. React components written in JSX
3. Babel to transform JSX to regular JavaScript

#### Components

**Component = A function that returns UI**

```jsx
function StrangelCard({ strangel, onClick }) {
    return (
        <div className="strangel-card" onClick={onClick}>
            <div className="strangel-name">{strangel.name}</div>
            <div className="strangel-title">{strangel.title}</div>
        </div>
    );
}
```

**Props** (`{ strangel, onClick }`) are inputs passed to components.

#### State

**State = Data that can change over time**

```jsx
function App() {
    // useState returns [currentValue, functionToUpdateIt]
    const [selectedStrangel, setSelectedStrangel] = useState(null);
    
    // When user clicks a card:
    const handleClick = (strangel) => {
        setSelectedStrangel(strangel);  // Updates state
        // React automatically re-renders the component
    };
}
```

#### useEffect — Running Code When Component Loads

```jsx
useEffect(() => {
    // This runs once when the component first appears
    async function loadData() {
        const strangels = await api.getStrangels();
        setStrangels(strangels);
    }
    loadData();
}, []);  // Empty array = run only once
```

#### Conditional Rendering

```jsx
{!selectedStrangel ? (
    // Show the grid of cards
    <div className="strangels-grid">
        {strangels.map((s) => <StrangelCard key={s.type} strangel={s} />)}
    </div>
) : selectedStrangel.type.includes('Heart') ? (
    // Show the Heart panel
    <HeartPanel onBack={handleBack} />
) : (
    // Show the petition panel for Fox/Furies/Nokso
    <PetitionPanel strangel={selectedStrangel} onBack={handleBack} />
)}
```

---

## How They Talk: The API

### What is an API?

**API (Application Programming Interface)** = A way for programs to communicate.

In web apps, this usually means:
1. Frontend sends an **HTTP request** to a URL
2. Backend processes it and sends back **JSON data**

### HTTP Methods

| Method | Purpose | Example |
|--------|---------|---------|
| GET | Retrieve data | Get list of Strangels |
| POST | Send data / trigger action | Submit a petition |
| PUT | Update existing data | (not used here) |
| DELETE | Remove data | (not used here) |

### Example: Touching the Heart

**Frontend sends:**
```javascript
fetch('/api/oracle/touch', { method: 'POST' })
```

**Backend receives** (in OracleController):
```csharp
[HttpPost("touch")]
public async Task<ActionResult<BlessingResponse>> TouchHeart()
{
    var blessing = await _oracleService.TouchHeartAsync();
    return Ok(blessing);
}
```

**Backend returns JSON:**
```json
{
    "id": "abc-123",
    "strangel": "The Woman with Heart",
    "message": "Something loosens in your chest...",
    "intensity": 0.4,
    "intensityDescription": "gentle, like a held breath",
    "durationMinutes": 29
}
```

**Frontend displays** the message to the user.

### API Endpoints in This App

| Endpoint | Method | What it does |
|----------|--------|--------------|
| `/api/oracle/presence` | GET | Check which Strangels are present |
| `/api/oracle/strangels` | GET | Get info about all Strangels |
| `/api/oracle/touch` | POST | Touch the Woman with Heart |
| `/api/oracle/petition/fox` | POST | Ask the Fox a question |
| `/api/oracle/confess` | POST | Confess to the Furies |
| `/api/oracle/invoke/nokso` | POST | Invoke Nok'so |

---

## Design Patterns Used

### 1. Clean Architecture

**What:** Separate code into layers with dependencies pointing inward.

**Why:** Changes to outer layers (UI, database) don't affect inner layers (business logic).

**In this app:**
- Domain (innermost) — knows nothing about other layers
- Application — knows about Domain
- Infrastructure — knows about Domain and Application
- API (outermost) — knows about everything

### 2. Strategy Pattern

**What:** Define a family of algorithms, encapsulate each one, and make them interchangeable.

**Why:** Each Strangel behaves differently, but the system treats them uniformly.

**In this app:**
```csharp
public interface IStrangelEngine
{
    StrangelType StrangelType { get; }
    Task<Blessing> GenerateBlessingAsync(string? petition = null);
}

// Four different implementations:
public class WomanWithHeartEngine : IStrangelEngine { /* always blesses */ }
public class FoxEngine : IStrangelEngine { /* sometimes tricks */ }
public class FuriesEngine : IStrangelEngine { /* always judges */ }
public class NoksoEngine : IStrangelEngine { /* disrupts or protects */ }
```

### 3. Dependency Injection (DI)

**What:** Instead of creating dependencies inside a class, they're passed in from outside.

**Why:** Makes code testable and flexible.

**In this app:**
```csharp
// Program.cs registers the services
builder.Services.AddScoped<IStrangelEngine, WomanWithHeartEngine>();

// OracleService receives them automatically
public class OracleService
{
    private readonly IEnumerable<IStrangelEngine> _engines;
    
    // ASP.NET automatically passes in all registered engines
    public OracleService(IEnumerable<IStrangelEngine> engines)
    {
        _engines = engines;
    }
}
```

### 4. Repository Pattern (prepared for, not fully implemented)

**What:** Abstract data access behind an interface.

**Why:** Could swap database implementations without changing business logic.

### 5. Value Objects

**What:** Immutable objects that represent a value rather than an identity.

**Why:** Prevents bugs from accidental modification, enforces validation.

**In this app:**
```csharp
// Can't create an invalid intensity
public sealed record BlessingIntensity
{
    private BlessingIntensity(double value)
    {
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException();
        Value = value;
    }
}
```

---

## Deployment: Docker and Railway

### What is Docker?

Docker packages your application with everything it needs to run (runtime, dependencies, etc.) into a **container**. The container runs the same way on any machine.

### The Dockerfile Explained

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore          # Download dependencies
COPY . .
RUN dotnet publish -o /app  # Compile the application

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY --from=build /app .    # Copy only the compiled output
ENTRYPOINT ["dotnet", "StrangelOracle.API.dll"]
```

**Multi-stage build:** Build stage has full SDK (large), runtime stage only has what's needed to run (small).

### What is Railway?

Railway is a **Platform as a Service (PaaS)** that:
1. Detects your Dockerfile
2. Builds the container
3. Deploys it to a server
4. Gives you a URL
5. Handles HTTPS, scaling, etc.

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
       │  (traffic routed)
       ▼
https://strangels.bw8.org
```

---

## Interview Talking Points

### When Asked "Tell me about this project"

> "I built a full-stack application in C# using ASP.NET Core for the backend and React for the frontend. It's based on a mythology I created for my poetry book.

> The backend follows Clean Architecture with four separate projects — Domain, Application, Infrastructure, and API. I used the Strategy Pattern for the different entity behaviors, where each Strangel has its own engine class that implements a common interface.

> The frontend is a single-page React application with component-based architecture. It communicates with the backend through a REST API.

> I containerized it with Docker and deployed to Railway with a custom domain through Cloudflare."

### Technical Questions You Might Get

**Q: Why Clean Architecture?**
> "It separates concerns and makes the codebase maintainable. The domain layer has no external dependencies, so business logic doesn't change if we swap databases or UI frameworks."

**Q: Explain the Strategy Pattern in your project.**
> "Each Strangel behaves differently — one always blesses, one sometimes refuses, one judges. Rather than putting all that logic in one class with conditionals, each has its own engine class implementing IStrangelEngine. The service iterates through them and picks the right one."

**Q: How does Dependency Injection work here?**
> "In Program.cs, I register all the Strangel engines with AddScoped. When OracleService is constructed, ASP.NET automatically injects all the registered IStrangelEngine implementations. The service doesn't need to know which concrete classes exist."

**Q: What's a Value Object?**
> "An immutable object defined by its values rather than an identity. BlessingIntensity is a good example — it wraps a double but ensures it's always between 0 and 1, and provides a ToDescription method. You can't accidentally modify it after creation."

**Q: How does the frontend communicate with the backend?**
> "Through fetch requests to the REST API. When the user touches the heart, the frontend POSTs to /api/oracle/touch. The controller calls the service, which calls the engine, which generates a blessing. The blessing is serialized to JSON and returned. The frontend updates React state, which triggers a re-render showing the message."

---

## Common Questions and Answers

### What is middleware?

Code that runs between receiving a request and sending a response. In `Program.cs`:

```csharp
app.UseDefaultFiles();   // Middleware 1: Check for index.html
app.UseStaticFiles();    // Middleware 2: Serve static files
app.MapControllers();    // Middleware 3: Route to controllers
```

Order matters! Static files must be checked before routing to controllers.

### What is JSON?

**JavaScript Object Notation** — A text format for data:

```json
{
    "name": "The Woman with Heart",
    "type": "WomanWithHeart",
    "isPresent": true
}
```

APIs typically send and receive JSON because it's human-readable and works with any programming language.

### What does `async/await` do?

Allows code to wait for operations (like database queries or API calls) without blocking:

```csharp
// Without async - blocks the thread
var data = GetDataFromDatabase();  

// With async - thread can do other work while waiting
var data = await GetDataFromDatabaseAsync();
```

### What is CORS?

**Cross-Origin Resource Sharing** — Security feature that controls which websites can call your API. We enabled it for the frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## Quick Reference

| Term | Definition |
|------|------------|
| **API** | Interface for programs to communicate |
| **REST** | Architectural style using HTTP methods (GET, POST, etc.) |
| **Controller** | Class that handles API requests |
| **Endpoint** | A specific URL that accepts requests |
| **Service** | Class containing business logic |
| **Entity** | Object with identity (like Blessing with an ID) |
| **Value Object** | Immutable object defined by values |
| **DTO** | Data Transfer Object — carries data between layers |
| **Dependency Injection** | Passing dependencies into classes rather than creating them |
| **Middleware** | Code that processes requests/responses in a pipeline |
| **SPA** | Single Page Application — one HTML file, JavaScript handles navigation |
| **Component** | Reusable piece of UI in React |
| **State** | Data that can change and triggers re-renders |
| **Props** | Data passed into a React component |

---

*You built this. You understand it. Go get that job.*
