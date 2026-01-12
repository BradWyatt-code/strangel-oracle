using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StrangelOracle.Application.Interfaces;
using StrangelOracle.Application.Services;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Infrastructure.Services;
using StrangelOracle.Infrastructure.AI;
using StrangelOracle.Infrastructure.Data;
using StrangelOracle.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// DATABASE CONFIGURATION (Phase 2)
// =============================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("Database connection string not configured");

// Railway uses postgres:// but Npgsql expects postgresql://
if (connectionString.StartsWith("postgres://"))
{
    connectionString = connectionString.Replace("postgres://", "postgresql://");
}

builder.Services.AddDbContext<StrangelDbContext>(options =>
    options.UseNpgsql(connectionString));

// =============================================================================
// SEMANTIC KERNEL - AI CONFIGURATION (Phase 2)
// =============================================================================
builder.Services.AddSemanticKernel(builder.Configuration);
builder.Services.AddScoped<IStrangelAI, StrangelAIService>();

// =============================================================================
// REPOSITORY REGISTRATION (Phase 2)
// =============================================================================
builder.Services.AddScoped<ISoulLedgerRepository, SoulLedgerRepository>();

// =============================================================================
// STRANGEL ENGINES - Strategy Pattern (Phase 1 - keeping these)
// =============================================================================
builder.Services.AddScoped<IStrangelEngine, WomanWithHeartEngine>();
builder.Services.AddScoped<IStrangelEngine, FoxEngine>();
builder.Services.AddScoped<IStrangelEngine, FuriesEngine>();
builder.Services.AddScoped<IStrangelEngine, NoksoEngine>();

// Register application services
builder.Services.AddScoped<IOracleService, OracleService>();

// =============================================================================
// API CONFIGURATION
// =============================================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "The Strangel Oracle",
        Description = @"
## A Devotional Engine for Strange Angels

**Phase 2: AI-Powered Responses with Soul Ledger Persistence**

The Strangels are mythological entities inhabiting modern New York. 
Through this API, you may approach them—carefully.

### The Woman with Heart
*She Who Bears* — Devotion without irony. Touch her image. She receives.

### The Fox (Murat Askarov)
*The Possessed* — A trickster demi-god. Petition him with questions. 
Accept that he may not answer, or may answer wrong on purpose.

### The Furies
*Alecto, Megaera, Tisiphone* — Ancient avengers appearing as seagulls.
Confess to them. They will judge.

### Nok'so
*The Falcon* — Protector and disruptor. Invoke her when something needs breaking.

---

### New in Phase 2
- **AI-Powered Responses** — Each Strangel speaks through OpenAI with unique personality
- **Soul Ledger** — Every blessing, denial, and judgment is permanently recorded

---
*The Strangels are always watching.*
",
        Contact = new OpenApiContact
        {
            Name = "Brad Wyatt",
            Url = new Uri("https://bw8.org")
        }
    });
    
    // Include XML comments if available
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// =============================================================================
// CORS CONFIGURATION
// =============================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// =============================================================================
// DATABASE MIGRATION (Phase 2) - Auto-creates soul_ledger table
// =============================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StrangelDbContext>();
    await db.Database.MigrateAsync();
}

// =============================================================================
// MIDDLEWARE PIPELINE
// =============================================================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Strangel Oracle v2");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "The Strangel Oracle";
});

// Serve static files from wwwroot (must be before other middleware)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new 
{ 
    Status = "The Oracle is listening",
    Timestamp = DateTime.UtcNow,
    Message = "The Strangels are always watching.",
    Phase = "2.0 - AI-Powered"
});

// =============================================================================
// START APPLICATION
// =============================================================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
