var builder = DistributedApplication.CreateBuilder(args);
var config = builder.Configuration;


//TODO: Include the BICEP to deploy Foundry resource with the  model gpt-4o-mini and the Document intelligence resource.
var openaiKey = config["Azure:OpenAI:ApiKey"];
var openaiKeyParameter = builder.AddParameter("openai-key", openaiKey!, secret: true);
var openaiEndpoint = config["Azure:OpenAI:EndPoint"];
var deploymentName = config["Azure:OpenAI:DeploymentName"];
var openai = builder.AddOpenAI("openai")
                    .WithEndpoint(openaiEndpoint!)
                    .WithApiKey(openaiKeyParameter);
var chat = openai.AddModel("chat", deploymentName!);

var documentIntelligenceEndpoint = config["Azure:DocumentIntelligence:EndPoint"];
var documentIntelligenceKey = config["Azure:DocumentIntelligence:ApiKey"];
var documentIntelligenceApiKeyParameter = builder.AddParameter("document-intelligence-api-key", documentIntelligenceKey!, secret: true);
var documentIntelligence = builder.AddProject<Projects.AgentFrameworkPocs_DocumentIntelligenceMcpServer>("document-intelligence-mcp")
    .WithEnvironment("DocumentIntelligence__Endpoint", documentIntelligenceEndpoint)
    .WithEnvironment("DocumentIntelligence__ApiKey", documentIntelligenceApiKeyParameter);

var dummyErp = builder.AddProject<Projects.AgentFrameworkPocs_DummyERPMCPServer>("dummy-erp-mcp");

var agent = builder.AddProject<Projects.AgentFrameworkPocs_Agent>("agent")
                   .WithEnvironment("AgentMode", "LlmHandOff")
                   .WithReference(documentIntelligence)
                   .WaitFor(documentIntelligence)
                   .WithReference(dummyErp)
                   .WaitFor(dummyErp)
                   .WithReference(chat)
                   .WaitFor(chat);

var mailpit = builder.AddMailPit("mailpit");

builder.AddProject<Projects.AgentFrameworkPocs_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(agent)
    .WaitFor(agent);

builder.Build().Run();