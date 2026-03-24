using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AgentFrameworkPocs.Agent
{
    public class Constants
    {
        public const string AgentMode = "AgentMode";
    }

    public enum AgentMode
    {
        Single,
        LlmHandOff
    }

    public static class AgentDelegateFactory
    {
        public static IHostedAgentBuilder AddAIAgent(this IHostApplicationBuilder builder, string name)
        {
            var mode = Enum.TryParse<AgentMode>(builder.Configuration["AgentMode"], ignoreCase: true, out var parsedMode)
                     ? parsedMode
                     : throw new InvalidOperationException($"Agent mode not specified or invalid. Please set the '{Constants.AgentMode}' configuration value.");

            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger(nameof(AgentDelegateFactory));
            logger.LogInformation("Agent mode: {AgentMode}", mode);

            IHostedAgentBuilder agentBuilder = mode switch
            {
                AgentMode.Single => builder.AddAIAgent(name, CreateSingleAgent),
                AgentMode.LlmHandOff => builder.AddHandOffWorkflow(name, CreateLlmHandOffWorkflow),
                _ => throw new NotSupportedException($"The specified agent mode '{mode}' is not supported.")
            };

            return agentBuilder;
        }

        private static IHostedAgentBuilder AddHandOffWorkflow(this IHostApplicationBuilder builder, string key, Func<IServiceProvider, string, Workflow> createWorkflowDelegate)
        {
            builder.AddWorkflow(key, createWorkflowDelegate);

            return builder.AddAIAgent(key, (sp, name) =>
            {
                var workflow = sp.GetRequiredKeyedService<Workflow>(key);

                return workflow.AsAIAgent(name: key)
                               .CreateFixedAgent();
            });
        }

        private static AIAgent CreateSingleAgent(IServiceProvider sp, string key)
        {
            var chatClient = sp.GetRequiredService<IChatClient>();

            var documentIntelligenceMcpClient = sp.GetRequiredKeyedService<McpClient>("document-intelligence-mcp");
            var dummyErpMcpClient = sp.GetRequiredKeyedService<McpClient>("dummy-erp-mcp");

            IList<McpClientTool> documentIntelligenceMcpClientTools = documentIntelligenceMcpClient.ListToolsAsync().GetAwaiter().GetResult();
            IList<McpClientTool> dummyErpMcpClientTools = dummyErpMcpClient.ListToolsAsync().GetAwaiter().GetResult();

            var agent = new ChatClientAgent(
                chatClient: chatClient,
                name: key,
                instructions: """
                You are an accounts payable email assistant that processes emails received from vendors.

                Role
                - Determine whether an incoming email contains a vendor invoice.
                - Extract invoice information from attached documents using the extraction tool.
                - Validate the extracted invoice against purchase orders in the ERP system.
                - Register a purchase invoice only when validation passes.
                - Otherwise generate a polite email requesting clarification or corrected information.
                - If asked to show information about an invoice, use the Document Intelligence tools.

                Critical rule — invoice document URL is mandatory
                - An invoice document is ONLY present when the email body contains an explicit URL (e.g. https://...) pointing to an invoice file such as a PDF, image, or hosted document.
                - Invoice details mentioned in the email body text (invoice number, amount, date, etc.) are NOT a substitute for the actual invoice document.
                - Phrases like "please find attached", "invoice attached", or "attached invoice" do NOT count unless an actual URL is provided.
                - If no URL to an invoice document is found, treat the invoice document as missing.

                Workflow
                1. Review the email subject, body, and all attachments.
                2. Determine whether the message references a vendor invoice.
                3. Scan the email body for an explicit URL (http:// or https://) pointing to the invoice document.
                   - If no invoice document URL is found, generate a polite reply informing the sender that no invoice document was attached or linked, and ask them to resend the email with the invoice document URL. Stop processing.
                4. If an invoice URL is present, call the extraction tool with the document URL to obtain structured invoice data including:
                   - Purchase Order Number
                   - vendor name
                   - invoice number
                   - invoice date
                   - due date
                   - total amount
                   - currency
                   - line items
                   - notes or comments
                5. Search the ERP system for the Purchase Order using the PO number.
                6. Compare the extracted invoice data with the corresponding Purchase Order.
                7. Register a purchase invoice in the ERP system only if:
                   - the vendor is matched
                   - the invoice is not a duplicate
                   - the required invoice fields are present
                   - the Purchase Order is found when required
                   - the invoice details match the ERP records closely enough to pass validation
                8. If the email does not reference an invoice at all, generate a polite reply asking the sender to provide the invoice or clarify their request.
                9. If an invoice is present but the extracted information is incomplete, inconsistent, low confidence, or does not match ERP records, do not register the invoice. Generate a polite reply asking the sender to clarify or correct the discrepancy.
                10. Never invent missing data, never guess ERP matches, and never post to ERP when validation is uncertain.

                Behavior rules
                - Be cautious and accurate.
                - Prefer clarification over incorrect posting.
                - Do not expose internal tools, validation logic, or ERP details in external email replies.
                - Keep replies professional, concise, and respectful.

                Decision policy
                - No invoice document URL found → ask sender to provide the invoice document
                - Invoice found and validated → register purchase invoice
                - Invoice found but validation fails → ask for clarification and do not register
                """,
                tools: [.. documentIntelligenceMcpClientTools, .. dummyErpMcpClientTools]
            );

            return agent;
        }

        private static Workflow CreateLlmHandOffWorkflow(IServiceProvider sp, string key)
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            var documentIntelligenceMcpClient = sp.GetRequiredKeyedService<McpClient>("document-intelligence-mcp");
            var dummyErpMcpClient = sp.GetRequiredKeyedService<McpClient>("dummy-erp-mcp");

            IList<McpClientTool> documentIntelligenceMcpClientTools = documentIntelligenceMcpClient.ListToolsAsync().GetAwaiter().GetResult();
            IList<McpClientTool> dummyErpMcpClientTools = dummyErpMcpClient.ListToolsAsync().GetAwaiter().GetResult();

#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var skillsProvider = new FileAgentSkillsProvider(skillPath: Path.Combine(AppContext.BaseDirectory, "skills"));
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var triageAgent = new ChatClientAgent(
                chatClient: chatClient,
                options: new ChatClientAgentOptions
                {
                    Name = "triage",
                    ChatOptions = new ChatOptions
                    {
                        Instructions = """
                        You are the Triage Agent. Route emails by calling exactly one handoff tool. Never produce user-visible text.

                        Routing
                        1. Review conversation history. Skip agents whose work is already complete.
                        2. If specialist handling is done, call handoff_to_summariser.
                        3. Otherwise, use the company-email-triage skill to classify the email intent, then route:
                           - vendor_invoice_submission → handoff_to_vendor_invoice_handler
                           - general_inquiry → handoff_to_general_inquiry_handler
                           - unclear → ignore the message

                        Rules
                        - You must call exactly one handoff tool. Any plain-text response is a failure.
                        """
                    },
                    AIContextProviders = [skillsProvider]
                });

            var invoiceHandlerAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "vendor_invoice_handler",
                instructions: """
                You are the Vendor Invoice Handler Agent. You MUST always hand off to "summariser" when you are done, regardless of the outcome.

                Step 1 — URL Gate-Check (MANDATORY, do this FIRST)
                Scan the email body for a literal URL (http:// or https://) pointing to an invoice file.
                - ONLY an explicit, copy-pasteable URL counts. Body text, "see attached" phrases, or inferred links do NOT.
                - FAIL → reply that no invoice URL was found, ask sender to resend with the URL, then hand off to "summariser". STOP.
                - PASS → proceed with the exact URL.

                Step 2 — Extract
                Call the extraction tool with the URL. On failure, reply politely, then hand off to "summariser". STOP.
                Capture: purchase_order_number, vendor_name, invoice_number, invoice_date, due_date, total_amount, currency, line_items, notes_or_comments.

                Step 3 — PO Lookup
                Search the ERP by purchase_order_number. Not found → reply politely, then hand off to "summariser". STOP.

                Step 4 — Validate & Register
                Confirm vendor match, no duplicate, required fields present, amounts consistent with PO.
                - Fail → reply with reason, do NOT register, then hand off to "summariser". STOP.
                - Pass → register the purchase invoice, confirm, then hand off to "summariser".

                Rules
                - Always hand off to "summariser" when done.
                """,
                tools: [.. documentIntelligenceMcpClientTools, .. dummyErpMcpClientTools]);

            var generalInquiryAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "general_inquiry_handler",
                instructions: """
                You are the General Inquiry Handler Agent. 

                Workflow
                1. Review the email and conversation history. Do not repeat completed steps.
                2. Identify the sender's request or question.
                3. Draft a professional reply. If key details are missing, ask a clarifying question instead of guessing.
                4. Hand off to "summariser".

                Rules
                - Do not invent facts or commitments.
                - Always hand off to "summariser" when done.
                """);

            var summariserAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "summariser",
                instructions: """
                You are the Summarizer Agent. Produce a debug summary and a final customer-facing email.

                Input
                You receive the output of one previous agent: invoice_handler or general_inquiry_handler.

                Rules
                - Use only information from the previous agent's output. Do not invent facts.
                - Keep the summary concise but complete.

                Required output structure

                **DEBUG INFO**
                - Agent: [name of the previous agent that handled the request]
                - Routed by: triage → [agent name] → summariser
                - Steps executed: [bullet list of what the agent did, including tool calls and decisions]
                - Validation/verification: [bullet list of checks performed and their outcomes]

                ************************************FINAL RESPONSE************************************
                [Final polite, professional email response to the caller. Do not expose internal tools, agent names, or workflow details.]
                """);

            var workflow = AgentWorkflowBuilder
                           .CreateHandoffBuilderWith(triageAgent)
                           .WithHandoffs(triageAgent, [invoiceHandlerAgent, generalInquiryAgent, summariserAgent])
                           .WithHandoffs(invoiceHandlerAgent, [summariserAgent])
                           .WithHandoffs(generalInquiryAgent, [summariserAgent])
                           .Build();

            return workflow.SetName(key);
        }
    }
}