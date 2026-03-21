using AgentFrameworkPocs.Agent;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHttpClient("document-intelligence-mcp", client =>
{
    client.BaseAddress = new Uri("https+http://document-intelligence-mcp");
});

builder.Services.AddKeyedSingleton<McpClient>("document-intelligence-mcp", (sp, obj) =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                       .CreateClient("document-intelligence-mcp");
    var endpoint = builder.Environment.IsDevelopment()
                 ? $"{httpClient.BaseAddress!.ToString().Replace("https+", string.Empty).TrimEnd('/')}"
                 : $"{httpClient.BaseAddress!.ToString().Replace("+http", string.Empty).TrimEnd('/')}";

    var clientTransport = new HttpClientTransport(
        new HttpClientTransportOptions { Endpoint = new Uri(endpoint) },
        httpClient,
        loggerFactory);

    var clientOptions = new McpClientOptions
    {
        ClientInfo = new Implementation
        {
            Name = "Document Intelligence Mcp Client",
            Version = "1.0.0",
        }
    };

    return McpClient.CreateAsync(clientTransport, clientOptions, loggerFactory).GetAwaiter().GetResult();
});

builder.Services.AddHttpClient("dummy-erp-mcp", client =>
{
    client.BaseAddress = new Uri("https+http://dummy-erp-mcp");
});

builder.Services.AddKeyedSingleton<McpClient>("dummy-erp-mcp", (sp, obj) =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                       .CreateClient("dummy-erp-mcp");
    var endpoint = builder.Environment.IsDevelopment()
                 ? $"{httpClient.BaseAddress!.ToString().Replace("https+", string.Empty).TrimEnd('/')}"
                 : $"{httpClient.BaseAddress!.ToString().Replace("+http", string.Empty).TrimEnd('/')}";

    var clientTransport = new HttpClientTransport(
        new HttpClientTransportOptions { Endpoint = new Uri(endpoint) },
        httpClient,
        loggerFactory);

    var clientOptions = new McpClientOptions
    {
        ClientInfo = new Implementation
        {
            Name = "Dummy ERP Mcp Client",
            Version = "1.0.0",
        }
    };

    return McpClient.CreateAsync(clientTransport, clientOptions, loggerFactory).GetAwaiter().GetResult();
});

builder.AddOpenAIClient("chat")
       .AddChatClient();

builder.AddAIAgent("email-agent");
builder.Services.AddAGUI();
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

app.MapAGUI(
    pattern: "ag-ui",
    aiAgent: app.Services.GetRequiredKeyedService<AIAgent>("email-agent")
);
app.MapOpenAIResponses();
app.MapOpenAIConversations();

if (builder.Environment.IsDevelopment())
{
    app.MapDevUI();
}

app.Run();