namespace PurchaseManagement.Application.BlanketMaster.Dto;

public sealed class GetBlanketMasterPendingGroupDto
{
    public int Id { get; set; }
    public string? BlanketNumber { get; set; }
    public DateTimeOffset BlanketDate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorMobile { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }

    // Workflow fields
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }
    public byte IsEdit { get; set; }

    public List<GetBlanketMasterPendingDto> Lines { get; set; } = new();
}
