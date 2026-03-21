using Azure;
using Azure.AI.DocumentIntelligence;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

internal class DocumentIntelligenceTools(DocumentIntelligenceClient client)
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    [McpServerTool]
    [Description("Extracts invoice data from a document at a given URL and outputs it as structured JSON.")]
    public async Task<string> AnalyzeDocumentAsync([Description("Url where the document is stored")] string url)
    {
        var invoiceUri = new Uri(url);

        Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(
            WaitUntil.Completed, "prebuilt-invoice", invoiceUri);

        AnalyzeResult result = operation.Value;

        var invoices = result.Documents.Select(doc => doc.Fields.ToDictionary(
            f => f.Key,
            f => ExtractFieldValue(f.Value)
        )).ToList();

        return JsonSerializer.Serialize(invoices, SerializerOptions);
    }

    private static object? ExtractFieldValue(DocumentField field)
    {
        if (field.FieldType == DocumentFieldType.String)
            return field.ValueString;
        if (field.FieldType == DocumentFieldType.Currency)
            return new { field.ValueCurrency.CurrencySymbol, field.ValueCurrency.Amount };
        if (field.FieldType == DocumentFieldType.Double)
            return field.ValueDouble;
        if (field.FieldType == DocumentFieldType.Date)
            return field.ValueDate;
        if (field.FieldType == DocumentFieldType.Int64)
            return field.ValueInt64;
        if (field.FieldType == DocumentFieldType.List)
            return field.ValueList.Select(ExtractFieldValue).ToList();
        if (field.FieldType == DocumentFieldType.Dictionary)
            return field.ValueDictionary.ToDictionary(f => f.Key, f => ExtractFieldValue(f.Value));
        return field.Content;
    }
}