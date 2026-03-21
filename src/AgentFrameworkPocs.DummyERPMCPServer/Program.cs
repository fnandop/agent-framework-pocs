var builder = WebApplication.CreateBuilder(args);

// Add the MCP services: the transport to use (http) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<PurchaseOrderTools>()
    .WithTools<PurchaseInvoiceTools>();

var app = builder.Build();
app.MapMcp();
app.UseHttpsRedirection();

app.Run();