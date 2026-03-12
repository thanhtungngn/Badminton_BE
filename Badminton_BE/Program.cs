using System;
using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Badminton_BE.Data;
using Badminton_BE.Repositories;
using Badminton_BE.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // Allow the local dev server and the deployed frontend (no trailing slash)
            policy.WithOrigins("http://localhost:5173", "https://badminton-web-lqny.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder.Services.AddControllers();
// Configure EF Core DbContext. If a connection string named "DefaultConnection" is provided it will use SQL Server,
// otherwise an in-memory database will be used for development/testing.
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(defaultConn))
{
    // Use MySQL (Pomelo) when DefaultConnection is provided for MySQL databases.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("BadmintonDb"));
}

// register repositories and services for DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ISessionPlayerRepository, SessionPlayerRepository>();

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ISessionPlayerService, SessionPlayerService>();
builder.Services.AddScoped<ISessionPaymentRepository, SessionPaymentRepository>();
builder.Services.AddScoped<IPlayerPaymentRepository, PlayerPaymentRepository>();
builder.Services.AddScoped<ISessionPaymentRepository, SessionPaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Learn more about configuring Swagger at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Badminton API",
        Version = "v1",
        Description = "Backend API for Badminton application"
    });

    // include XML comments if present
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in Development or when the environment variable `ENABLE_SWAGGER` is set to "true".
var enableSwagger = app.Environment.IsDevelopment() ||
                    string.Equals(builder.Configuration["ENABLE_SWAGGER"], "true", StringComparison.OrdinalIgnoreCase);

// Log environment and critical configuration flags for debugging on Render
try
{
    var startupLogger = app.Logger;
    startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    var enableSwaggerVar = builder.Configuration["ENABLE_SWAGGER"];
    startupLogger.LogInformation("ENABLE_SWAGGER env var (raw): {EnableSwagger}", enableSwaggerVar ?? "<null>");
    startupLogger.LogInformation("Swagger enabled: {Enabled}", enableSwagger);
    // Do not log sensitive connection strings. Log only presence for troubleshooting.
    startupLogger.LogInformation("DefaultConnection configured: {HasDefaultConnection}", !string.IsNullOrEmpty(defaultConn));
}
catch (Exception ex)
{
    try { app.Logger.LogError(ex, "Error while logging startup diagnostics"); } catch { }
}

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Badminton API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();


// Apply any pending EF Core migrations at startup for relational databases.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
        var migrationLogger = loggerFactory?.CreateLogger("DatabaseMigration");

        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (db.Database.IsRelational())
            {
                migrationLogger?.LogInformation("Applying database migrations...");
                db.Database.Migrate();
                migrationLogger?.LogInformation("Database migrations applied.");
            }
            else
            {
                migrationLogger?.LogInformation("Database provider is not relational; skipping migrations.");
            }
        }
        catch (Exception ex)
        {
            migrationLogger?.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}
catch (Exception)
{
    // If migration fails, rethrow to prevent the application from starting in a bad state.
    throw;
}

app.MapControllers();

app.Run();
