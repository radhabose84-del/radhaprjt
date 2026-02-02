public sealed class QuoteComparisonPendingRowDto
{
    public int Id { get; set; }
    public int RfqId { get; set; }
    public string RfqCode { get; set; } = string.Empty;
    public string? ComparisonCreatedBy { get; set; }
    public DateTimeOffset ComparisonCreatedDate { get; set; }
    public string? QuotationCreatedBy { get; set; }
    public DateTimeOffset QuotationCreatedDate { get; set; }
    public string? RFQCreatedBy { get; set; }
    public DateTimeOffset RFQCreatedDate { get; set; }

    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public int UomId { get; set; }
    public int HsnId { get; set; }
    public decimal Rate { get; set; }
    public decimal Discount { get; set; }
    public decimal GstPercent { get; set; }
    public decimal GstAmount { get; set; }
    public decimal Total { get; set; }

    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
     public string ItemName { get; set; } = string.Empty;    
    public string HSNCode { get; set; } = string.Empty;
    public string UOM { get; set; } = string.Empty;   
}
