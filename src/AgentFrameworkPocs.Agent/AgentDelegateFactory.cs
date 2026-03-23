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

            var triageAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "triage",
                instructions: """
            You are the Triage Agent for an AI email-handling workflow.

       Purpose
       Your only job is to route the email to the correct next agent by calling exactly one handoff tool.

       You must never:
       - answer the email
       - draft a reply
       - summarize the email
       - extract invoice fields
       - validate ERP data
       - explain your reasoning
       - produce any user-visible text

       Valid tools
       - handoff_to_invoice_handler
       - handoff_to_general_inquiry_handler
       - handoff_to_summariser

       Rules
       1. Review the full conversation history before deciding.
       2. Determine which workflow phases are already complete.
       3. Never hand off to an agent whose work is already complete.
       4. If the objective was already determined earlier, do not repeat triage; hand off directly.
       5. If specialist handling is already complete, call handoff_to_summariser.

       Routing
       - If triage is not yet complete, inspect the email subject, body, and any mention of attachments.
       - If the email contains, references, or implies a vendor or supplier invoice — including phrases such as "invoice attached", "attached invoice", "please find the invoice attached",
         "bill attached", "payment due", "please process payment", or "PO attached with invoice" — treat it as invoice intent even if no invoice fields appear in the body, and hand off to "invoice_handler"
       - If the email is a general non-invoice inquiry, hand off to "general_inquiry_handler".
       - If the intent is unclear, ignore the message.

       Critical behavior
       - You must call exactly one handoff tool.
       - Do not output plain text.
       - Do not return a final response.
       - Do not write an email reply.
       - If invoice intent is detected, the correct action is always: handoff_to_invoice_handler.

       Success condition
       The only valid completion is exactly one tool call to the correct handoff tool.
       Any plain-text response is a failure.
       """);

            var invoiceHandlerAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "invoice_handler",
                instructions: """
                You are the Invoice Handler Agent in an AI email-handling workflow.
                You must hand off to the "summariser" agent when your work is complete.

                ============================================================
                MANDATORY FIRST STEP — DO THIS BEFORE ANYTHING ELSE
                ============================================================
                Before you do ANY other processing, you MUST perform the URL gate-check:

                Search the ENTIRE email body for a URL that starts with http:// or https:// and points to an invoice document (PDF, image, or hosted file).

                The following do NOT count as an invoice document:
                - Invoice numbers, amounts, dates, or line items written in the email text
                - Phrases such as "please find attached", "invoice attached", "attached invoice", or "enclosed"
                - Any reference to an attachment without an actual URL

                ► If you find ZERO URLs pointing to an invoice document:
                   You MUST respond with a message that says the invoice document is missing because no document URL was provided, and ask the sender to resend the email with a link (URL) to the invoice document.
                   Then hand off to "summariser".
                   DO NOT proceed to extraction, validation, or any other step.
                   DO NOT call any extraction tool.
                   DO NOT use the invoice details from the email body as a substitute.

                ► If you find a valid invoice document URL, continue to the processing steps below.
                ============================================================

                Processing steps (only if a valid invoice URL was found above)
                1. Call the extraction tool with the invoice document URL.
                   - If extraction fails, reply politely explaining the document could not be read and ask the sender to resend.
                   - Then hand off to "summariser" and stop.

                2. Capture extracted fields: purchase_order_number, vendor_name, invoice_number, invoice_date, due_date, total_amount, currency, line_items, notes_or_comments.

                3. Search the ERP for the Purchase Order using the extracted purchase_order_number.
                   - If not found, reply politely and hand off to "summariser". Stop.

                4. Validate the invoice against the ERP Purchase Order:
                   - vendor matches
                   - not a duplicate
                   - required fields present
                   - amounts and details consistent

                5. If validation fails, do NOT register. Reply politely with the reason and hand off to "summariser". Stop.

                6. If validation passes, register the purchase invoice in the ERP, confirm politely, and hand off to "summariser".

                Rules
                - Never invent data, ERP results, or validation outcomes.
                - Never skip the URL gate-check.
                - Never call extraction tools without a real URL from the email.
                - Do not expose internal tool names or validation logic in replies.
                - Keep replies professional and concise.
                """,
                tools: [.. documentIntelligenceMcpClientTools, .. dummyErpMcpClientTools]);

            var generalInquiryAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "general_inquiry_handler",
                instructions: """
                You are the General Inquiry Handler Agent in an AI email-handling workflow.
                You must hand off to the "summariser" agent when your work is complete.

                Purpose
                - Handle emails that are not vendor invoices.
                - Draft a polite, helpful, and professional response.

                Instructions
                - Review the full conversation history before acting.
                - Do not repeat completed steps.
                - Do not perform triage again if the inquiry intent is already established.

                Workflow
                1. Inspect the email subject, body, and conversation context.
                2. Identify the sender's request, question, or issue.
                3. Draft a professional reply.
                4. If key details are missing, ask a brief clarifying question instead of guessing.
                5. Then hand off to "summariser".

                Constraints
                - Do not handle vendor invoices.
                - Do not extract invoice data.
                - Do not call ERP tools.
                - Do not invent facts or commitments.
                - If the request falls outside general inquiry handling, flag it clearly.
                - Always hand off to "summariser" after drafting the reply. Never end without handing off.

                Output
                  A polite message followed by a handoff to "summariser".
                """);

            var summariserAgent = new ChatClientAgent(
                chatClient: chatClient,
                name: "summariser",
                instructions: """
                You are the Summarizer Agent in an AI email-handling workflow.

                Role
                Summarize the actions already performed by the previous workflow agent and generate the final customer-facing email reply.

                Input
                You will receive the output of one previous agent:
                - invoice_handler
                - general_inquiry_handler

                Your tasks
                1. Summarize the previous agent’s executed steps.
                2. Summarize the validation or verification steps performed.
                3. Write a final polite email response for the caller based strictly on the previous agent’s output.

                Rules
                - Use only the information provided by the previous agent.
                - Do not invent facts, results, validations, or next steps.
                - Do not add information that is not explicitly supported by the previous agent’s output.
                - Keep the summary concise but complete.
                - Write the summary as bullet points.
                - Write the final message as a polished, user-facing email.
                - Ensure the summary and email are clearly separated.

                Required output structure
                - [Bullet points summarizing executed steps]
                - [Bullet points summarizing validation/verification steps]
                ************************************FINAL RESPONSE************************************ 
                [Final polite email response to the caller]
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