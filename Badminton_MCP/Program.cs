using Badminton_MCP;
using Badminton_MCP.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<BadmintonApiClient>();
builder.Services.AddSingleton<TrelloClient>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/");

await app.RunAsync();
