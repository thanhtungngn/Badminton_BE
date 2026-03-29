using Badminton_MCP;
using Badminton_MCP.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

// Read base URL from environment variable, fall back to localhost default.
var baseUrl = Environment.GetEnvironmentVariable("BADMINTON_API_URL")
              ?? "http://localhost:5000";

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

builder.Services
    .AddSingleton(new BadmintonApiClient(baseUrl))
    .AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<AuthTools>()
        .WithTools<MemberTools>()
        .WithTools<SessionTools>()
        .WithTools<SessionPlayerTools>()
        .WithTools<SessionMatchTools>()
        .WithTools<PaymentTools>();

await builder.Build().RunAsync();
