namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;
public sealed class QuoteComparisonPendingGroupDto
{
    public int Id { get; set; } 
    public int RfqId { get; set; }
    public string RfqCode { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }  
    public byte IsEdit { get; set; }
    public string? ComparisonCreatedBy { get; set; }
    public DateTimeOffset ComparisonCreatedDate { get; set; }
    public string? QuotationCreatedBy { get; set; }
    public DateTimeOffset QuotationCreatedDate { get; set; }
    public string? RFQCreatedBy { get; set; }
    public DateTimeOffset RFQCreatedDate { get; set; }
    public List<QuoteComparisonPendingDto> Lines { get; set; } = new();
}
