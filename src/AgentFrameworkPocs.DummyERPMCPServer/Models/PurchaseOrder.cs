namespace AgentFrameworkPocs.DummyERPMCPServer.Models;

/// <summary>
/// Represents a purchase order matching the Business Central purchaseOrder entity structure.
/// </summary>
internal class PurchaseOrder
{
    public required string Id { get; set; }
    public required string Number { get; set; }
    public required string OrderDate { get; set; }
    public required string PostingDate { get; set; }
    public required string DueDate { get; set; }
    public required string VendorId { get; set; }
    public required string VendorNumber { get; set; }
    public required string VendorName { get; set; }
    public required string PayToName { get; set; }
    public required string PayToVendorId { get; set; }
    public required string PayToVendorNumber { get; set; }
    public required string ShipToName { get; set; }
    public required string ShipToContact { get; set; }
    public required string BuyFromAddressLine1 { get; set; }
    public string BuyFromAddressLine2 { get; set; } = "";
    public required string BuyFromCity { get; set; }
    public required string BuyFromCountry { get; set; }
    public required string BuyFromState { get; set; }
    public required string BuyFromPostCode { get; set; }
    public required string ShipToAddressLine1 { get; set; }
    public string ShipToAddressLine2 { get; set; } = "";
    public required string ShipToCity { get; set; }
    public required string ShipToCountry { get; set; }
    public required string ShipToState { get; set; }
    public required string ShipToPostCode { get; set; }
    public required string PayToAddressLine1 { get; set; }
    public string PayToAddressLine2 { get; set; } = "";
    public required string PayToCity { get; set; }
    public required string PayToCountry { get; set; }
    public required string PayToState { get; set; }
    public required string PayToPostCode { get; set; }
    public required string CurrencyId { get; set; }
    public required string CurrencyCode { get; set; }
    public bool PricesIncludeTax { get; set; }
    public required string PaymentTermsId { get; set; }
    public required string ShipmentMethodId { get; set; }
    public required string Purchaser { get; set; }
    public required string RequestedReceiptDate { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool DiscountAppliedBeforeTax { get; set; } = true;
    public decimal TotalAmountExcludingTax { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalAmountIncludingTax { get; set; }
    public bool FullyReceived { get; set; }
    public required string Status { get; set; }
    public required string LastModifiedDateTime { get; set; }
}