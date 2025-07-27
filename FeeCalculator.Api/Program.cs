using FeeCalculator.Core.Interfaces;
using FeeCalculator.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Register services with dependency injection
builder.Services.AddScoped<IFeeCalculationService, FeeCalculationService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fee Calculator API",
        Version = "v1",
        Description = "A flexible, rule-based fee calculation engine for payment transactions with dynamic tariff management",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@company.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Use XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS for development and frontend integration (e.g. Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:4200") // Angular dev server
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure logging levels
builder.Logging.AddFilter("FeeCalculator", LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fee Calculator API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app root
        c.DocumentTitle = "Fee Calculator API Documentation";
        c.DefaultModelsExpandDepth(-1); // Collapse models by default
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse endpoints by default
    });
    
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
    app.UseHsts(); // Add HSTS header for security
}

app.UseHttpsRedirection();

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
    Environment = app.Environment.EnvironmentName
});

// Startup info including environment and service status
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Fee Calculator API started at {Time}", DateTime.UtcNow);
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Swagger UI available at: {SwaggerUrl}", app.Environment.IsDevelopment() ? "https://localhost:7000" : "N/A");

// Pre-warm the fee calculation service
try
{
    using var scope = app.Services.CreateScope();
    var feeService = scope.ServiceProvider.GetRequiredService<IFeeCalculationService>();
    logger.LogInformation("Fee calculation service initialized successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize fee calculation service");
}

app.Run();