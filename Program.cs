using accapp001.Components;
using Microsoft.EntityFrameworkCore;
using accapp001.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for Azure App Service
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Entity Framework Core with Azure SQL Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Support multiple connection string sources for Azure App Service
    var connectionString = GetAzureConnectionString(builder.Configuration);
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Configure for Azure SQL Database with enhanced retry policy
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(60),
            errorNumbersToAdd: new[] { 2, 53, 64, 233, 10053, 10054, 10060, 40197, 40501, 40613, 49918, 49919, 49920 });
        
        // Optimize for Azure SQL Database
        sqlOptions.CommandTimeout(300); // 5 minutes for Azure SQL
        sqlOptions.MigrationsAssembly("accapp001");
    });
    
    // Configure based on environment
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
    else
    {
        // Production optimizations for Azure
        options.EnableServiceProviderCaching();
        options.EnableDetailedErrors(false);
    }
});

// Add health checks for Azure App Service monitoring
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready" });

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure database for Azure deployment
await ConfigureAzureDatabaseAsync(app);

// Configure the HTTP request pipeline for Azure App Service
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Azure App Service health probe endpoints
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Helper method to get connection string from Azure App Service configuration
static string GetAzureConnectionString(IConfiguration configuration)
{
    // Priority order for Azure App Service:
    // 1. Azure App Service Connection Strings (highest priority)
    // 2. Azure Key Vault (if configured)
    // 3. Environment Variables
    // 4. appsettings.json (development fallback)
    
    // Azure App Service automatically maps connection strings to configuration
    var connectionString = configuration.GetConnectionString("EmployeeDatabase");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        // Fallback to environment variable (common in Azure containers)
        connectionString = configuration["AZURE_SQL_CONNECTIONSTRING"] ?? 
                          configuration["DATABASE_CONNECTION_STRING"];
    }
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "No database connection string found. Please configure 'EmployeeDatabase' connection string in Azure App Service Configuration or set AZURE_SQL_CONNECTIONSTRING environment variable.");
    }
    
    return connectionString;
}

// Configure database for Azure App Service deployment
static async Task ConfigureAzureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database configuration for Azure deployment...");
        
        // Test database connectivity
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogWarning("Cannot connect to database. Will retry during first request.");
            return;
        }
        
        // Apply migrations in production (Azure App Service)
        if (app.Environment.IsProduction())
        {
            logger.LogInformation("Applying database migrations for production deployment...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations completed successfully.");
        }
        else
        {
            // Development: ensure database exists
            logger.LogInformation("Ensuring database exists for development environment...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database creation completed.");
        }
        
        // Verify database health
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogWarning("Pending migrations detected: {Migrations}", string.Join(", ", pendingMigrations));
        }
        else
        {
            logger.LogInformation("Database is up to date with all migrations applied.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database configuration failed during startup. The application will continue but database operations may fail.");
        
        // In Azure App Service, don't fail startup - allow the app to start and handle DB errors gracefully
        if (app.Environment.IsProduction())
        {
            logger.LogWarning("Production environment: Application will start despite database configuration issues.");
        }
        else
        {
            // In development, we might want to know about DB issues immediately
            throw new InvalidOperationException("Database configuration failed during startup.", ex);
        }
    }
}
