namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public sealed class GetBlanketPOPendingGroupDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = default!;
    public int POMethodId { get; set; }
    public DateTimeOffset PODate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorMobile { get; set; }
    public decimal PurchaseValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? POCategoryCode { get; set; }
    public string? POMethodCode { get; set; }
    public decimal ItemTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal PandFTotal { get; set; }
    public decimal MiscCharges { get; set; }
    public decimal GSTTotal { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }

    // Blanket reference
    public int? BlanketHeaderId { get; set; }
    public string? BlanketNumber { get; set; }

    // Workflow fields
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }
    public byte IsEdit { get; set; }

    public List<GetBlanketPOPendingDto> Lines { get; set; } = new();
}
