namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;

public sealed class GetContractPOPendingGroupDto
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
    public decimal GSTPercentage { get; set; }
    public decimal CGSTPercentage { get; set; }
    public decimal SGSTPercentage { get; set; }
    public decimal IGSTPercentage { get; set; }
    public decimal? CGSTTotal { get; set; }
    public decimal? SGSTTotal { get; set; }
    public decimal? IGSTTotal { get; set; }
    public decimal FreightTotal { get; set; }
    public decimal InsuranceTotal { get; set; }
    public decimal TDSTotal { get; set; }
    public decimal AdvanceAmount { get; set; }
    public DateTimeOffset createdDate { get; set; }
    public string createdByName { get; set; } = default!;

    // Contract-specific fields
    public int? ContractPOHeaderId { get; set; }
    public string? ContractPONumber { get; set; }

    // Workflow / approval fields
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }
    public byte IsEdit { get; set; }

    public List<GetContractPOPendingDto> Lines { get; set; } = new();
}
