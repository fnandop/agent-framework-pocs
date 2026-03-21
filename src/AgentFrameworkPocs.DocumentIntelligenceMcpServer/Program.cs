using Azure;
using Azure.AI.DocumentIntelligence;

var builder = WebApplication.CreateBuilder(args);

var diEndpoint = builder.Configuration["DocumentIntelligence:Endpoint"]
    ?? throw new InvalidOperationException("DocumentIntelligence:Endpoint configuration is required.");
var diKey = builder.Configuration["DocumentIntelligence:ApiKey"]
    ?? throw new InvalidOperationException("DocumentIntelligence:ApiKey configuration is required.");

builder.Services.AddSingleton(new DocumentIntelligenceClient(
    new Uri(diEndpoint), new AzureKeyCredential(diKey)));

// Add the MCP services: the transport to use (http) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<DocumentIntelligenceTools>();

var app = builder.Build();
app.MapMcp();
app.UseHttpsRedirection();

app.Run();