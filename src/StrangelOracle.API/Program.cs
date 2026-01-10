using StrangelOracle.Application.Interfaces;
using StrangelOracle.Application.Services;
using StrangelOracle.Domain.Interfaces;
using StrangelOracle.Infrastructure.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Register Strangel engines (Strategy pattern)
builder.Services.AddScoped<IStrangelEngine, WomanWithHeartEngine>();
builder.Services.AddScoped<IStrangelEngine, FoxEngine>();
builder.Services.AddScoped<IStrangelEngine, FuriesEngine>();
builder.Services.AddScoped<IStrangelEngine, NoksoEngine>();

// Register application services
builder.Services.AddScoped<IOracleService, OracleService>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "The Strangel Oracle",
        Description = @"
## A Devotional Engine for Strange Angels

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

// Configure CORS for frontend
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

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Strangel Oracle v1");
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
    Message = "The Strangels are always watching."
});



app.Run();
