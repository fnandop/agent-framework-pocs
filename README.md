# Agent Framework PoCs + Azure Document Intelligence — Accounts Payable Email Processing

A proof-of-concept that demonstrates how to build a **multi-agent email-handling system** using the [Azure Agent Framework](https://github.com/microsoft/agent-framework), [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers, [Azure Document Intelligence](https://learn.microsoft.com/azure/ai-services/document-intelligence/), and [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) for orchestration.

The scenario simulates an **accounts payable** department that receives vendor emails, automatically extracts invoice data from attached documents, validates invoices against ERP purchase orders, and either registers the invoice or drafts a polite reply asking for corrections.

> **Note:** Some of these steps could be implemented deterministically using tools such as Logic Apps, Durable Functions, or a rule engine (or some ERPs might already have similar features). However, the goal of this PoC is to explore how an agent-based architecture can handle complex, multi-step workflows that mix deterministic and generative steps, and how MCP servers can be used to integrate with backend systems in a modular way.

---

## Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                        .NET Aspire AppHost                         │
│                    (orchestrates all services)                     │
└──────┬──────────┬──────────┬──────────┬──────────┬─────────────────┘
       │          │          │          │          │
       ▼          ▼          ▼          ▼          ▼
  ┌─────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
  │   Web   │ │ Agent  │ │Doc Int.│ │Dummy   │ │MailPit │
  │Frontend │ │Service │ │  MCP   │ │Server  │ │(Email) │
  │(Blazor) │ │        │ │Server  │ │Server  │ │        │
  └────┬────┘ └───┬────┘ └────────┘ └────────┘ └────────┘
       │          │
       │   AG-UI  │
       └──────────┘
```

### Projects

| Project | Description |
|---|---|
| **AgentFrameworkPocs.AppHost** | .NET Aspire host — wires all services, passes configuration and secrets |
| **AgentFrameworkPocs.Agent** | Agent service hosting the AI workflow, exposes the [AG-UI](https://github.com/microsoft/agent-framework) SSE endpoint |
| **AgentFrameworkPocs.Web** | Blazor Server frontend with an interactive chat UI for testing email handling |
| **AgentFrameworkPocs.DocumentIntelligenceMcpServer** | MCP server wrapping Azure Document Intelligence (`prebuilt-invoice` model) |
| **AgentFrameworkPocs.DummyERPMCPServer** | MCP server simulating an ERP with mocked purchase orders and invoices (modeled after Business Central APIs) |
| **AgentFrameworkPocs.ServiceDefaults** | Shared Aspire service defaults (health checks, telemetry, service discovery) |

---

## Agent Modes

The agent service supports two modes, configured via the `AgentMode` environment variable:

### Single Agent

A single `ChatClientAgent` that handles the full email processing workflow end-to-end:

1. Receives the email
2. Checks for an invoice document URL
3. Extracts data via Document Intelligence
4. Validates against the ERP
5. Registers the purchase invoice or drafts a clarification email

### LLM Hand-Off (Multi-Agent Workflow)

A **four-agent handoff workflow** where each agent is responsible for a specific phase:

```
         ┌───────────────┐
         │  Triage Agent  │
         └──────┬────────┘
                │
        ┌───────┴────────┐
        ▼                ▼
┌──────────────┐  ┌─────────────────┐
│   Invoice    │  │ General Inquiry │
│   Handler    │  │    Handler      │
└──────┬───────┘  └───────┬─────────┘
       │                  │
       └───────┬──────────┘
               ▼
       ┌──────────────┐
       │  Summariser  │
       └──────────────┘
```

| Agent | Role |
|---|---|
| **Triage** | Classifies the email (invoice vs. general inquiry) and routes to the correct specialist. Produces no user-visible output — only calls a handoff tool. |
| **Invoice Handler** | Validates that an invoice document URL exists, extracts data using Document Intelligence, validates against ERP purchase orders, and either registers the invoice or explains the failure. |
| **General Inquiry Handler** | Drafts a polite, professional reply for non-invoice emails. |
| **Summariser** | Takes the specialist's output and produces the final sender-facing email response. |

---

## Email Processing Workflow

```
Incoming Email
     │
     ▼
 ┌──────────────┐
 │ Triage: Is   │
 │ it an invoice?│
 └──────┬───────┘
    Yes │        No
        ▼         ▼
  ┌───────────┐  ┌──────────────────┐
  │ URL check │  │ General Inquiry  │
  │ (gate)    │  │ → draft reply    │
  └─────┬─────┘  └────────┬─────────┘
  Found │  Missing         │
        ▼    ▼             │
   ┌────────┐ Ask for      │
   │Extract │ document     │
   │invoice │              │
   └───┬────┘              │
       ▼                   │
  ┌──────────┐             │
  │ Lookup   │             │
  │ PO in ERP│             │
  └────┬─────┘             │
       ▼                   │
  ┌──────────┐             │
  │ Validate │             │
  └────┬─────┘             │
  Pass │  Fail             │
       ▼    ▼              │
  Register  Ask for        │
  invoice   correction     │
       │       │           │
       └───────┴───────────┘
                │
                ▼
         ┌────────────┐
         │ Summariser │
         │ → final    │
         │   email    │
         └────────────┘
```

---

## MCP Servers

### Document Intelligence MCP Server

Wraps [Azure AI Document Intelligence](https://learn.microsoft.com/azure/ai-services/document-intelligence/) and exposes one MCP tool:

| Tool | Description |
|---|---|
| `AnalyzeDocumentAsync` | Accepts a document URL, runs the `prebuilt-invoice` model, and returns structured invoice data as JSON |

### Dummy ERP MCP Server

Simulates an ERP system (modeled after [Business Central APIs v2.0](https://learn.microsoft.com/dynamics365/business-central/dev-itpro/api-reference/v2.0/)) with in-memory mock data:

**Purchase Order tools:**

| Tool | Description |
|---|---|
| `GetPurchaseOrders` | Returns all purchase orders |
| `GetPurchaseOrder` | Gets a single PO by order number |
| `SearchPurchaseOrdersByVendor` | Searches POs by vendor name |
| `SearchPurchaseOrdersByStatus` | Searches POs by status |

**Purchase Invoice tools:**

| Tool | Description |
|---|---|
| `GetPurchaseInvoices` | Returns all purchase invoices |
| `GetPurchaseInvoice` | Gets a single invoice by number |
| `CreatePurchaseInvoice` | Creates a new purchase invoice (auto-generates ID and number, sets status to Draft) |
| `SearchPurchaseInvoicesByVendor` | Searches invoices by vendor name |
| `SearchPurchaseInvoicesByStatus` | Searches invoices by status |

---

## Tech Stack

- **.NET 10** / **C# 14**
- **Blazor Server** — Interactive chat UI with SSE streaming
- **.NET Aspire 13.1** — Service orchestration, service discovery, health checks
- **Azure OpenAI** — `gpt-4o-mini` deployment for agent reasoning
- **Azure Document Intelligence** — `prebuilt-invoice` model for invoice extraction
- **Microsoft.Agents.AI** — Agent framework (ChatClientAgent, Workflows, Handoffs)
- **AG-UI Protocol** — SSE-based streaming protocol for agent ↔ frontend communication
- **Model Context Protocol (MCP)** — Tool interoperability between agents and backend services
- **MailPit** — Local email server for testing (via [CommunityToolkit.Aspire.Hosting.MailPit](https://github.com/CommunityToolkit/Aspire))

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (required by Aspire for MailPit)
- An **Azure AI Foundry** resource with a deployed `gpt-4o-mini` model
- An **Azure Document Intelligence** resource

> **Note:** For this PoC the Azure AI Foundry and Document Intelligence resources were created directly in the Azure Portal. Bicep templates for automated provisioning will be provided in a future update (see [Next Steps](#next-steps)).

---

## Configuration

API keys are stored in [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for the AppHost project and endpoints in appsettings or env variables.

```json
"Azure": {
  "OpenAI": {
    "EndPoint": "https://<your-resource>.openai.azure.com/openai/v1",
    "DeploymentName": "gpt-4o-mini",
    "ApiKey": ""      // in user secrets as "Azure:OpenAI:ApiKey"
  },
  "DocumentIntelligence": {
    "Endpoint": "https://<your-resource>.cognitiveservices.azure.com/",
    "ApiKey": ""      // in user secrets as "Azure:DocumentIntelligence:ApiKey"
  }
}
```

Set the secrets for the AppHost project (`AgentFrameworkPocs.AppHost`):

```bash
cd AgentFrameworkPocs.AppHost

dotnet user-secrets set "Azure:OpenAI:ApiKey" "<your-openai-api-key>"
dotnet user-secrets set "Azure:DocumentIntelligence:ApiKey" "<your-doc-intelligence-api-key>"

```

---

## Running

```bash
cd AgentFrameworkPocs.AppHost
dotnet run
```

The Aspire dashboard will launch automatically. From there you can access:

- **Web Frontend** — Blazor chat UI to paste email bodies and test the agent
- **Aspire Dashboard** — Monitor all services, logs, and traces
- **MailPit** — Inspect test emails
- **Agent DevUI** — Built-in agent debugging UI (development only)

### Testing the Agent

1. Open the **Web Frontend** from the Aspire dashboard
2. Paste an email body into the input area
3. The agent will process the email and return a response

**Example — Invoice with document URL:**
```
Subject: Invoice for PO-3333

Hi,

Please find our invoice for Purchase Order PO-3333.
The invoice document is available at: https://example.com/invoices/INV-100.pdf

Total amount: $165.00 USD, due by December 15, 2024.

Best regards,
Contoso Ltd.
```

**Example — Invoice without document URL (should be rejected):**
```
Hi,

Please find attached Invoice IVN0001.
The total amount due is 4343, with payment due by 22/03/2026.

Thank you.
```

**Example — General inquiry:**
```
Hi,

Could you please confirm the delivery address for our next shipment?

Thanks,
John
```

---

## Next Steps

- [ ] **RAG with Azure AI Search** — Enhance the General Inquiry Handler with Retrieval-Augmented Generation using Azure AI Search to answer questions based on company knowledge base documents
- [ ] **Bicep templates** — Infrastructure-as-code to deploy the Azure AI Foundry resource with the `gpt-4o-mini` model and the Document Intelligence resource
- [ ] **Email integration** — Connect MailPit to the agent for full end-to-end email processing
- [ ] **Persistent conversation** — Store conversation history across sessions

---

## References

This PoC was built using the following repositories as reference and documentation:

- **[agent-framework](https://github.com/microsoft/agent-framework)** — The [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) used as the core agent runtime, including AG-UI hosting, handoff workflows, and ChatClientAgent
- **[interview-coach-agent-framework](https://github.com/Azure-Samples/interview-coach-agent-framework)** — An Azure Samples project that served as a starting point for the multi-agent handoff pattern, MCP client integration with Aspire, and the AG-UI endpoint wiring

---

## License

This project is a proof of concept for experimentation purposes.
