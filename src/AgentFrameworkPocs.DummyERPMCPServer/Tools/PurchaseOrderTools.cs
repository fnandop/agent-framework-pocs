using AgentFrameworkPocs.DummyERPMCPServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

/// <summary>
/// MCP tools that return mocked purchase order data,
/// modeled after the Business Central purchaseOrders API (v2.0).
/// </summary>
internal class PurchaseOrderTools
{
    private static readonly List<PurchaseOrder> MockPurchaseOrders =
    [
        new PurchaseOrder
        {
            Id = "5d115c9c-44e3-ea11-bb43-000d3a2feca1",
            Number = "PO-108001",
            OrderDate = "2025-01-15",
            PostingDate = "2025-01-15",
            DueDate = "2025-02-15",
            VendorId = "a1b2c3d4-0001-0001-0001-000000000001",
            VendorNumber = "20000",
            VendorName = "First Up Consultants",
            PayToName = "First Up Consultants",
            PayToVendorId = "a1b2c3d4-0001-0001-0001-000000000001",
            PayToVendorNumber = "20000",
            ShipToName = "Contoso Ltd.",
            ShipToContact = "Evan McIntosh",
            BuyFromAddressLine1 = "100 Day Drive",
            BuyFromCity = "Chicago",
            BuyFromCountry = "US",
            BuyFromState = "IL",
            BuyFromPostCode = "61236",
            ShipToAddressLine1 = "100 Day Drive",
            ShipToCity = "Chicago",
            ShipToCountry = "US",
            ShipToState = "IL",
            ShipToPostCode = "61236",
            PayToAddressLine1 = "100 Day Drive",
            PayToCity = "Chicago",
            PayToCountry = "US",
            PayToState = "IL",
            PayToPostCode = "61236",
            CurrencyId = "00000000-0000-0000-0000-000000000000",
            CurrencyCode = "USD",
            PricesIncludeTax = false,
            PaymentTermsId = "04a5738a-44e3-ea11-bb43-000d3a2feca1",
            ShipmentMethodId = "93f5638a-55e3-4a22-aa32-211d3a2fdce5",
            Purchaser = "Evan McIntosh",
            RequestedReceiptDate = "2025-02-01",
            DiscountAmount = 0m,
            DiscountAppliedBeforeTax = true,
            TotalAmountExcludingTax = 3122.80m,
            TotalTaxAmount = 187.37m,
            TotalAmountIncludingTax = 3310.17m,
            FullyReceived = true,
            Status = "Completed",
            LastModifiedDateTime = "2025-01-20"
        },
        new PurchaseOrder
        {
            Id = "7e226b1d-55f4-eb22-cc54-111e4b3gfdb2",
            Number = "PO-108002",
            OrderDate = "2025-03-10",
            PostingDate = "2025-03-10",
            DueDate = "2025-04-10",
            VendorId = "b2c3d4e5-0002-0002-0002-000000000002",
            VendorNumber = "30000",
            VendorName = "Fabrikam Inc.",
            PayToName = "Fabrikam Inc.",
            PayToVendorId = "b2c3d4e5-0002-0002-0002-000000000002",
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
            PaymentTermsId = "15b6849b-55f4-eb22-cc54-111e4b3gfdb2",
            ShipmentMethodId = "a4g6749b-66f4-5b33-bb43-322e5c4hgef6",
            Purchaser = "Maria Campbell",
            RequestedReceiptDate = "2025-03-25",
            DiscountAmount = 150.00m,
            DiscountAppliedBeforeTax = true,
            TotalAmountExcludingTax = 8750.00m,
            TotalTaxAmount = 525.00m,
            TotalAmountIncludingTax = 9275.00m,
            FullyReceived = false,
            Status = "In Review",
            LastModifiedDateTime = "2025-03-15"
        },
        new PurchaseOrder
        {
            Id = "9f337c2e-66g5-fc33-dd65-222f5c4hgec3",
            Number = "PO-108003",
            OrderDate = "2025-05-20",
            PostingDate = "2025-05-20",
            DueDate = "2025-06-20",
            VendorId = "c3d4e5f6-0003-0003-0003-000000000003",
            VendorNumber = "40000",
            VendorName = "Northwind Traders",
            PayToName = "Northwind Traders",
            PayToVendorId = "c3d4e5f6-0003-0003-0003-000000000003",
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
            PaymentTermsId = "26c795ac-66g5-fc33-dd65-222f5c4hgec3",
            ShipmentMethodId = "b5h7850c-77g5-6c44-cc54-433f6d5ihfg7",
            Purchaser = "James Parker",
            RequestedReceiptDate = "2025-06-05",
            DiscountAmount = 500.00m,
            DiscountAppliedBeforeTax = false,
            TotalAmountExcludingTax = 15400.00m,
            TotalTaxAmount = 924.00m,
            TotalAmountIncludingTax = 16324.00m,
            FullyReceived = false,
            Status = "Open",
            LastModifiedDateTime = "2025-05-22"
        },
        new PurchaseOrder
        {
            Id = "af448d3f-77h6-gd44-ee76-333g6d5ihfd4",
            Number = "PO-3333",
            OrderDate = "2019-11-15",
            PostingDate = "2019-11-15",
            DueDate = "2019-12-15",
            VendorId = "d4e5f6g7-0004-0004-0004-000000000004",
            VendorNumber = "50000",
            VendorName = "CONTOSO LTD.",
            PayToName = "Microsoft Finance",
            PayToVendorId = "d4e5f6g7-0004-0004-0004-000000000004",
            PayToVendorNumber = "50000",
            ShipToName = "Microsoft Corp",
            ShipToContact = "Microsoft Corp",
            BuyFromAddressLine1 = "123 Bill St",
            BuyFromCity = "Redmond",
            BuyFromCountry = "US",
            BuyFromState = "WA",
            BuyFromPostCode = "98052",
            ShipToAddressLine1 = "123 Other St",
            ShipToCity = "Redmond",
            ShipToCountry = "US",
            ShipToState = "WA",
            ShipToPostCode = "98052",
            PayToAddressLine1 = "123 Bill St",
            PayToCity = "Redmond",
            PayToCountry = "US",
            PayToState = "WA",
            PayToPostCode = "98052",
            CurrencyId = "00000000-0000-0000-0000-000000000000",
            CurrencyCode = "USD",
            PricesIncludeTax = false,
            PaymentTermsId = "37d8a6bd-77h6-gd44-ee76-333g6d5ihfd4",
            ShipmentMethodId = "c6i8961d-88h6-7d55-dd65-544g7e6jigj8",
            Purchaser = "Microsoft Finance",
            RequestedReceiptDate = "2019-12-15",
            DiscountAmount = 0m,
            DiscountAppliedBeforeTax = true,
            TotalAmountExcludingTax = 100.00m,
            TotalTaxAmount = 10.00m,
            TotalAmountIncludingTax = 610.00m,
            FullyReceived = false,
            Status = "Open",
            LastModifiedDateTime = "2019-11-15"
        }
    ];

    [McpServerTool]
    [Description("Retrieves a list of all purchase orders. Returns purchase order details including vendor info, addresses, amounts, and status.")]
    public List<PurchaseOrder> GetPurchaseOrders()
    {
        return MockPurchaseOrders;
    }

    [McpServerTool]
    [Description("Retrieves a single purchase order by its order number (e.g. 'PO-108001'). Returns null if not found.")]
    public PurchaseOrder? GetPurchaseOrder(
        [Description("The purchase order number to look up (e.g. 'PO-108001').")] string orderNumber)
    {
        return MockPurchaseOrders.Find(po => po.Number.Equals(orderNumber, StringComparison.OrdinalIgnoreCase));
    }

    [McpServerTool]
    [Description("Searches purchase orders by vendor name. Returns all purchase orders whose vendor name contains the search text (case-insensitive).")]
    public List<PurchaseOrder> SearchPurchaseOrdersByVendor(
        [Description("The vendor name or partial name to search for.")] string vendorName)
    {
        return MockPurchaseOrders
            .Where(po => po.VendorName.Contains(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    [McpServerTool]
    [Description("Searches purchase orders by status. Valid statuses: 'Open', 'In Review', 'Completed'.")]
    public List<PurchaseOrder> SearchPurchaseOrdersByStatus(
        [Description("The status to filter by (e.g. 'Open', 'In Review', 'Completed').")] string status)
    {
        return MockPurchaseOrders
            .Where(po => po.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}