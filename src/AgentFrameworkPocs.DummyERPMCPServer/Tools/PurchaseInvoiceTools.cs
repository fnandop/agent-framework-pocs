using AgentFrameworkPocs.DummyERPMCPServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

/// <summary>
/// MCP tools that manage mocked purchase invoice data,
/// modeled after the Business Central purchaseInvoices API (v2.0).
/// </summary>
internal class PurchaseInvoiceTools
{
    private static readonly List<PurchaseInvoice> MockPurchaseInvoices =
    [
        new PurchaseInvoice
        {
            Id = "5d115c9c-44e3-ea11-bb43-000d3a2feca1",
            Number = "108001",
            InvoiceDate = "2019-01-01",
            PostingDate = "2019-01-01",
            DueDate = "2019-01-01",
            VendorInvoiceNumber = "107001",
            VendorId = "f8a5738a-44e3-ea11-bb43-000d3a2feca1",
            VendorNumber = "20000",
            VendorName = "First Up Consultants",
            PayToName = "First Up Consultants",
            PayToContact = "Evan McIntosh",
            PayToVendorId = "f8a5738a-44e3-ea11-bb43-000d3a2feca1",
            PayToVendorNumber = "20000",
            ShipToName = "CRONUS International Ltd.",
            BuyFromAddressLine1 = "100 Day Drive",
            BuyFromCity = "Chicago",
            BuyFromCountry = "US",
            BuyFromState = "IL",
            BuyFromPostCode = "61236",
            ShipToAddressLine1 = "7122 South Ashford Street",
            ShipToAddressLine2 = "Westminster",
            ShipToCity = "Atlanta",
            ShipToCountry = "US",
            ShipToPostCode = "31772",
            PayToAddressLine1 = "100 Day Drive",
            PayToCity = "Chicago",
            PayToCountry = "US",
            PayToState = "IL",
            PayToPostCode = "61236",
            CurrencyId = "00000000-0000-0000-0000-000000000000",
            CurrencyCode = "USD",
            PricesIncludeTax = false,
            DiscountAmount = 0m,
            DiscountAppliedBeforeTax = false,
            TotalAmountExcludingTax = 3122.80m,
            TotalTaxAmount = 187.37m,
            TotalAmountIncludingTax = 3310.17m,
            Status = "Draft",
            LastModifiedDateTime = "2020-08-21T00:26:53.793Z"
        },
        new PurchaseInvoice
        {
            Id = "6e226d2d-55f4-eb22-cc54-111e4b3feca2",
            Number = "108002",
            InvoiceDate = "2024-06-15",
            PostingDate = "2024-06-15",
            DueDate = "2024-07-15",
            VendorInvoiceNumber = "107002",
            VendorId = "a9b6849b-55f4-eb22-cc54-111e4b3feca2",
            VendorNumber = "30000",
            VendorName = "Fabrikam Inc.",
            PayToName = "Fabrikam Inc.",
            PayToContact = "Maria Campbell",
            PayToVendorId = "a9b6849b-55f4-eb22-cc54-111e4b3feca2",
            PayToVendorNumber = "30000",
            ShipToName = "Contoso Ltd.",
            ShipToContact = "Maria Campbell",
            BuyFromAddressLine1 = "456 Innovation Blvd",
            BuyFromCity = "Seattle",
            BuyFromCountry = "US",
            BuyFromState = "WA",
            BuyFromPostCode = "98101",
            ShipToAddressLine1 = "789 Commerce St",
            ShipToCity = "Redmond",
            ShipToCountry = "US",
            ShipToState = "WA",
            ShipToPostCode = "98052",
            PayToAddressLine1 = "456 Innovation Blvd",
            PayToCity = "Seattle",
            PayToCountry = "US",
            PayToState = "WA",
            PayToPostCode = "98101",
            CurrencyId = "00000000-0000-0000-0000-000000000000",
            CurrencyCode = "USD",
            PricesIncludeTax = false,
            DiscountAmount = 150.00m,
            DiscountAppliedBeforeTax = true,
            TotalAmountExcludingTax = 8750.00m,
            TotalTaxAmount = 525.00m,
            TotalAmountIncludingTax = 9275.00m,
            Status = "Paid",
            LastModifiedDateTime = "2024-07-01T14:30:00.000Z"
        },
        new PurchaseInvoice
        {
            Id = "7f337e3e-66a5-fc33-dd65-222f5c4feca3",
            Number = "108003",
            InvoiceDate = "2025-03-20",
            PostingDate = "2025-03-20",
            DueDate = "2025-04-20",
            VendorInvoiceNumber = "107003",
            VendorId = "b0c795ac-66a5-fc33-dd65-222f5c4feca3",
            VendorNumber = "40000",
            VendorName = "Northwind Traders",
            PayToName = "Northwind Traders",
            PayToContact = "James Parker",
            PayToVendorId = "b0c795ac-66a5-fc33-dd65-222f5c4feca3",
            PayToVendorNumber = "40000",
            ShipToName = "Contoso Ltd.",
            ShipToContact = "James Parker",
            BuyFromAddressLine1 = "321 Lakeside Ave",
            BuyFromCity = "Minneapolis",
            BuyFromCountry = "US",
            BuyFromState = "MN",
            BuyFromPostCode = "55401",
            ShipToAddressLine1 = "654 Central Pkwy",
            ShipToCity = "St. Paul",
            ShipToCountry = "US",
            ShipToState = "MN",
            ShipToPostCode = "55102",
            PayToAddressLine1 = "321 Lakeside Ave",
            PayToCity = "Minneapolis",
            PayToCountry = "US",
            PayToState = "MN",
            PayToPostCode = "55401",
            CurrencyId = "00000000-0000-0000-0000-000000000000",
            CurrencyCode = "USD",
            PricesIncludeTax = true,
            DiscountAmount = 0m,
            DiscountAppliedBeforeTax = true,
            TotalAmountExcludingTax = 4500.00m,
            TotalTaxAmount = 270.00m,
            TotalAmountIncludingTax = 4770.00m,
            Status = "Open",
            LastModifiedDateTime = "2025-03-22T09:15:00.000Z"
        }
    ];

    private static int _nextNumber = 108004;

    [McpServerTool]
    [Description("Retrieves a list of all purchase invoices. Returns invoice details including vendor info, addresses, amounts, and status.")]
    public List<PurchaseInvoice> GetPurchaseInvoices()
    {
        return MockPurchaseInvoices;
    }

    [McpServerTool]
    [Description("Retrieves a single purchase invoice by its invoice number (e.g. '108001'). Returns null if not found.")]
    public PurchaseInvoice? GetPurchaseInvoice(
        [Description("The invoice number to look up (e.g. '108001').")] string invoiceNumber)
    {
        return MockPurchaseInvoices.Find(inv => inv.Number.Equals(invoiceNumber, StringComparison.OrdinalIgnoreCase));
    }

    [McpServerTool]
    [Description("Creates a new purchase invoice. Returns the created invoice with a generated id, number, and status of 'Draft'. " +
        "Modeled after the Business Central POST purchaseInvoices API (v2.0).")]
    public PurchaseInvoice CreatePurchaseInvoice(
        [Description("Invoice date (e.g. '2025-01-01').")] string invoiceDate,
        [Description("Posting date (e.g. '2025-01-01').")] string postingDate,
        [Description("Due date (e.g. '2025-02-01').")] string dueDate,
        [Description("Vendor invoice number (e.g. '107001').")] string vendorInvoiceNumber,
        [Description("Vendor ID (GUID).")] string vendorId,
        [Description("Vendor number (e.g. '20000').")] string vendorNumber,
        [Description("Pay-to vendor ID (GUID).")] string payToVendorId,
        [Description("Pay-to vendor number (e.g. '20000').")] string payToVendorNumber,
        [Description("Buy-from address line 1.")] string buyFromAddressLine1,
        [Description("Buy-from city.")] string buyFromCity,
        [Description("Buy-from country code (e.g. 'US').")] string buyFromCountry,
        [Description("Buy-from state (e.g. 'IL').")] string buyFromState,
        [Description("Buy-from postal code.")] string buyFromPostCode,
        [Description("Ship-to address line 1.")] string shipToAddressLine1,
        [Description("Ship-to city.")] string shipToCity,
        [Description("Ship-to country code (e.g. 'US').")] string shipToCountry,
        [Description("Ship-to postal code.")] string shipToPostCode,
        [Description("Currency code (e.g. 'USD').")] string currencyCode = "USD",
        [Description("Whether prices include tax.")] bool pricesIncludeTax = false,
        [Description("Discount amount.")] decimal discountAmount = 0,
        [Description("Whether discount is applied before tax.")] bool discountAppliedBeforeTax = true,
        [Description("Total amount including tax.")] decimal totalAmountIncludingTax = 0,
        [Description("Ship-to name.")] string shipToName = "",
        [Description("Ship-to contact.")] string shipToContact = "",
        [Description("Buy-from address line 2.")] string buyFromAddressLine2 = "",
        [Description("Ship-to address line 2.")] string shipToAddressLine2 = "",
        [Description("Ship-to state.")] string shipToState = "",
        [Description("Currency ID (GUID). Defaults to empty GUID for local currency.")] string currencyId = "00000000-0000-0000-0000-000000000000")
    {
        var invoice = new PurchaseInvoice
        {
            Id = Guid.NewGuid().ToString(),
            Number = _nextNumber++.ToString(),
            InvoiceDate = invoiceDate,
            PostingDate = postingDate,
            DueDate = dueDate,
            VendorInvoiceNumber = vendorInvoiceNumber,
            VendorId = vendorId,
            VendorNumber = vendorNumber,
            VendorName = "",
            PayToName = "",
            PayToVendorId = payToVendorId,
            PayToVendorNumber = payToVendorNumber,
            ShipToName = shipToName,
            ShipToContact = shipToContact,
            BuyFromAddressLine1 = buyFromAddressLine1,
            BuyFromAddressLine2 = buyFromAddressLine2,
            BuyFromCity = buyFromCity,
            BuyFromCountry = buyFromCountry,
            BuyFromState = buyFromState,
            BuyFromPostCode = buyFromPostCode,
            ShipToAddressLine1 = shipToAddressLine1,
            ShipToAddressLine2 = shipToAddressLine2,
            ShipToCity = shipToCity,
            ShipToCountry = shipToCountry,
            ShipToState = shipToState,
            ShipToPostCode = shipToPostCode,
            PayToAddressLine1 = buyFromAddressLine1,
            PayToAddressLine2 = buyFromAddressLine2,
            PayToCity = buyFromCity,
            PayToCountry = buyFromCountry,
            PayToState = buyFromState,
            PayToPostCode = buyFromPostCode,
            CurrencyId = currencyId,
            CurrencyCode = currencyCode,
            PricesIncludeTax = pricesIncludeTax,
            DiscountAmount = discountAmount,
            DiscountAppliedBeforeTax = discountAppliedBeforeTax,
            TotalAmountExcludingTax = 0m,
            TotalTaxAmount = 0m,
            TotalAmountIncludingTax = totalAmountIncludingTax,
            Status = "Draft",
            LastModifiedDateTime = DateTime.UtcNow.ToString("o")
        };

        MockPurchaseInvoices.Add(invoice);
        return invoice;
    }

    [McpServerTool]
    [Description("Searches purchase invoices by vendor name. Returns all invoices whose vendor name contains the search text (case-insensitive).")]
    public List<PurchaseInvoice> SearchPurchaseInvoicesByVendor(
        [Description("The vendor name or partial name to search for.")] string vendorName)
    {
        return MockPurchaseInvoices
            .Where(inv => inv.VendorName.Contains(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    [McpServerTool]
    [Description("Searches purchase invoices by status. Valid statuses: 'Draft', 'Open', 'Paid'.")]
    public List<PurchaseInvoice> SearchPurchaseInvoicesByStatus(
        [Description("The status to filter by (e.g. 'Draft', 'Open', 'Paid').")] string status)
    {
        return MockPurchaseInvoices
            .Where(inv => inv.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}