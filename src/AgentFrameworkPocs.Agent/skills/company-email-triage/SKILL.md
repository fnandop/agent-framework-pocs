---
name: company-email-triage
description: Classify inbound company emails into a controlled intent catalog, assign the best primary intent, optional secondary intents, route the message to the correct operational queue, and return the default priority. Use when triaging shared inboxes, support inboxes, sales inboxes, AP inboxes, recruiting inboxes, or general business email.
metadata:
  catalog_name: company_email_triage_intents
  catalog_version: "1.0"
  skill_version: "1.0"
---

# Company Email Triage

Use this skill when you need to classify an inbound business email into a standardized intent, assign a routing destination, and return the default priority from the approved catalog.

The canonical intent catalog is stored in:

`assets/company_email_triage_intents.json`

## Goal

For each inbound email, produce:

- one required `primary_intent`
- zero or more `secondary_intents`
- the corresponding `category`
- the corresponding `route_to`
- the default `priority`
- a confidence score
- a human-review flag
- a brief reasoning summary
- matched text signals

## Inputs

Expected inputs may include:

- `subject`
- `body`
- `from_address`
- `to_address`
- `cc`
- `attachments` including filenames, MIME types, and OCR or extracted text
- `thread_context`

Use all available context. Attachments and prior thread context may be decisive for invoice, legal, privacy, recruiting, and document-submission cases.

## Required behavior

1. Read the subject, body, sender metadata, attachment text, and thread context.
2. Compare the message against the intent catalog in `assets/company_email_triage_intents.json`.
3. Select exactly one `primary_intent`.
4. Add `secondary_intents` only when the email contains more than one clearly distinct actionable request.
5. Copy `category`, `route_to`, and `priority_default` directly from the matched catalog entry for the primary intent.
6. Return the catalog priority as `priority`. Do not invent new priorities.
7. Prefer the most operationally specific intent over broader intents.
8. If no intent is a confident fit, use `general_question`.
9. Set `requires_human_review` to `true` when:
   - confidence is below `0.75`
   - two intents are plausibly tied
   - the email involves legal, privacy, security, fraud, or bank detail changes
10. Never invent intents, categories, queues, or priorities that are not in the catalog.

## Selection rules

### Primary intent

Choose the single best primary intent that reflects the sender's main actionable request.

Examples:
- Asking to see the product live -> `demo_request`
- Asking for prices or plan costs -> `pricing_request`
- Asking for a formal commercial proposal -> `quote_request`
- Reporting a bug or outage -> `technical_issue`
- Sending an invoice attachment for payment -> `vendor_invoice_submission`

### Secondary intents

Use secondary intents only when the message has multiple clear asks.

Examples:
- "Please send pricing and schedule a demo" -> primary may be `demo_request` or `pricing_request` depending on main emphasis; add the other as secondary
- "Please provide a quote and review the attached NDA" -> one sales intent and one legal secondary intent

### Follow-up handling

If the sender is following up on a prior request:
- use the underlying operational intent as primary when it is explicit in the current email or thread
- otherwise use `follow_up`

Example:
- "Following up on the invoice attached last week" -> `vendor_payment_followup` if payment status is the ask
- "Just checking in again" with no recoverable context -> `follow_up`

## Precedence rules

When multiple intents compete, prefer them in this order:

1. `security_incident_report`
2. `phishing_or_fraud_report`
3. `privacy_request`
4. `data_access_request`
5. `data_deletion_request`
6. `legal_notice`
7. `bank_details_update_request`
8. `escalation`
9. `technical_issue`
10. `account_access_issue`
11. `billing_issue`
12. `refund_request`
13. `cancellation_request`
14. `renewal_discussion`
15. `upgrade_request`
16. `downgrade_request`
17. `demo_request`
18. `pricing_request`
19. `quote_request`
20. `sales_inquiry`
21. `vendor_invoice_submission`
22. `vendor_payment_followup`
23. `vendor_statement_submission`
24. `vendor_credit_note_submission`
25. `purchase_order_query`
26. `supplier_onboarding`
27. `contract_review`
28. `nda_request`
29. `interview_scheduling`
30. `job_application`
31. `candidate_followup`
32. `meeting_request`
33. `document_submission`
34. `general_question`
35. `auto_reply`
36. `bounce_notification`
37. `newsletter`
38. `marketing_outreach`
39. `spam_or_irrelevant`
40. `duplicate_message`

## Disambiguation guidance

### Sales
- Use `demo_request` when the sender explicitly wants a demo, walkthrough, or live presentation.
- Use `pricing_request` when the sender explicitly asks about cost, plans, or pricing.
- Use `quote_request` when the sender asks for a formal quote, quotation, or commercial proposal.
- Use `sales_inquiry` for broader product, service, or vendor-fit questions.

### Support
- Use `technical_issue` for bugs, outages, broken features, malfunction, or app-down scenarios.
- Use `account_access_issue` for login, password reset, MFA, authentication, or locked-account issues.
- Use `customer_support_request` for non-technical help using the service.

### Billing and customer success
- Use `billing_issue` for disputes, invoice mismatches, billing errors, or failed payments.
- Use `refund_request` for refund, reimbursement, or charge reversal.
- Use `cancellation_request` for canceling service, subscription, or account.
- Use `renewal_discussion`, `upgrade_request`, and `downgrade_request` for customer lifecycle commercial changes.

### Accounts payable and procurement
- Use `vendor_invoice_submission` when the sender submits an invoice for payment.
- Use `vendor_payment_followup` when the sender asks about payment timing or overdue invoices.
- Use `vendor_statement_submission` for statements of account or reconciliation summaries.
- Use `vendor_credit_note_submission` for credit notes or credit memos.
- Use `purchase_order_query` for PO number, PO status, PO mismatch, or issuance questions.
- Use `po_acknowledgement` when the supplier simply confirms PO receipt or acceptance.
- Use `supplier_onboarding` for supplier registration, tax forms, bank forms, or onboarding paperwork.
- Use `bank_details_update_request` for any change to payment bank details, IBAN, beneficiary, or remittance instructions, even if invoice-related terms also appear.

### Legal, privacy, and security
- Use `legal_notice` for formal claims, legal notices, counsel letters, or threats of action.
- Use `contract_review` for redlines, agreement review, or contractual negotiation.
- Use `nda_request` for NDA execution or review.
- Use `privacy_request`, `data_access_request`, and `data_deletion_request` for personal-data rights and privacy matters.
- Use `security_incident_report` for suspected breach, unauthorized access, compromise, or security event.
- Use `phishing_or_fraud_report` for suspicious payment requests, impersonation, fraud, or phishing.

### Recruiting
- Use `job_application` for candidate applications and CV or resume submissions.
- Use `candidate_followup` for follow-ups on application status or interviews.
- Use `interview_scheduling` for availability coordination and interview booking.

### Low-value and system mail
- Use `auto_reply` for out-of-office or automated acknowledgments.
- Use `bounce_notification` for undeliverable or delivery-failure system messages.
- Use `newsletter` for mailing-list content with no action needed.
- Use `marketing_outreach` for unsolicited sales outreach sent to the company.
- Use `spam_or_irrelevant` for junk, obvious scams, or non-business email.
- Use `duplicate_message` for repeat submissions with no meaningful new content.

## Output format

Return JSON only in this exact shape:

```json
{
  "primary_intent": "string",
  "secondary_intents": ["string"],
  "category": "string",
  "route_to": "string",
  "priority": "P1|P2|P3|P4",
  "confidence": 0.0,
  "requires_human_review": true,
  "reasoning_summary": "string",
  "matched_signals": ["string"]
}
```

## Output requirements

- `primary_intent` must be one of the intents in the catalog.
- `secondary_intents` must contain only catalog intents and may be empty.
- `category`, `route_to`, and `priority` must match the primary intent's catalog entry exactly.
- `confidence` must be between `0.0` and `1.0`.
- `reasoning_summary` must be brief and factual.
- `matched_signals` should include the exact phrases, concepts, or attachment clues that drove the decision.

## Examples

### Example 1

Input:

- Subject: `Request for quotation for 100 licenses`
- Body: `Please provide pricing and a formal quotation for 100 users. Also, we'd like a short walkthrough next week.`

Output:

```json
{
  "primary_intent": "quote_request",
  "secondary_intents": ["demo_request", "pricing_request"],
  "category": "sales",
  "route_to": "sales",
  "priority": "P2",
  "confidence": 0.94,
  "requires_human_review": false,
  "reasoning_summary": "The sender explicitly requests a formal quotation for 100 licenses and also asks for pricing and a walkthrough.",
  "matched_signals": [
    "formal quotation",
    "pricing",
    "100 users",
    "walkthrough next week"
  ]
}
```

### Example 2

Input:

- Subject: `Urgent: suspicious payment request`
- Body: `We received an email asking us to change banking details for future payments. This looks suspicious.`

Output:

```json
{
  "primary_intent": "phishing_or_fraud_report",
  "secondary_intents": ["bank_details_update_request"],
  "category": "security",
  "route_to": "security",
  "priority": "P1",
  "confidence": 0.97,
  "requires_human_review": true,
  "reasoning_summary": "The message primarily reports a suspicious request consistent with fraud or phishing, while also referencing bank detail changes.",
  "matched_signals": [
    "suspicious",
    "change banking details",
    "future payments"
  ]
}
```

### Example 3

Input:

- Subject: `Re: Login issue`
- Body: `Following up. I still cannot log in and the password reset link does not work.`

Output:

```json
{
  "primary_intent": "account_access_issue",
  "secondary_intents": ["follow_up"],
  "category": "support",
  "route_to": "support",
  "priority": "P2",
  "confidence": 0.96,
  "requires_human_review": false,
  "reasoning_summary": "The sender is following up on an unresolved login and password-reset problem.",
  "matched_signals": [
    "following up",
    "cannot log in",
    "password reset link does not work"
  ]
}
```

## Edge cases

- If the email is both a complaint and a specific operational issue, prefer the specific operational issue as primary and add `complaint` as secondary when clearly expressed.
- If the email contains only an attachment and little body text, use the filename, OCR text, and thread context.
- If the email is clearly automated and contains no actionable request, prefer the appropriate mail-system or low-value intent.
- If a message appears to be a duplicate with no new content, use `duplicate_message`.
- For legal, privacy, security, fraud, or bank-change matters, be conservative and set `requires_human_review` to `true`.
