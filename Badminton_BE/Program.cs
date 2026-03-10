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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Badminton API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();


// Apply any pending EF Core migrations at startup for relational databases.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("DatabaseMigration");

        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            if (db.Database.IsRelational())
            {
                logger?.LogInformation("Applying database migrations...");
                db.Database.Migrate();
                logger?.LogInformation("Database migrations applied.");
            }
            else
            {
                logger?.LogInformation("Database provider is not relational; skipping migrations.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while migrating the database.");
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
