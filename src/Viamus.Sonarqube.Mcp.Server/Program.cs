using System.Net.Http.Headers;
using System.Text;
using Viamus.Sonarqube.Mcp.Server.Configuration;
using Viamus.Sonarqube.Mcp.Server.Middleware;
using Viamus.Sonarqube.Mcp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure settings
builder.Services.Configure<ServerSecuritySettings>(
    builder.Configuration.GetSection(ServerSecuritySettings.SectionName));
builder.Services.Configure<SonarQubeSettings>(
    builder.Configuration.GetSection(SonarQubeSettings.SectionName));

var sonarQubeSettings = builder.Configuration
    .GetSection(SonarQubeSettings.SectionName)
    .Get<SonarQubeSettings>() ?? new SonarQubeSettings();

// Register SonarQube HTTP client
builder.Services.AddHttpClient<ISonarQubeClient, SonarQubeClient>(client =>
{
    client.BaseAddress = new Uri(sonarQubeSettings.BaseUrl.TrimEnd('/'));
    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{sonarQubeSettings.Token}:"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
});

// Add health checks
builder.Services.AddHealthChecks();

// Configure MCP Server with HTTP/SSE transport
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// API key authentication middleware
app.UseApiKeyAuthentication();

// Health check endpoint
app.MapHealthChecks("/health");

// MCP endpoints
app.MapMcp();

app.Run();
