namespace PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

public sealed class GetContractPOMasterPendingGroupDto
{
    public int Id { get; set; }
    public string? ContractPONumber { get; set; }
    public DateTimeOffset ContractDate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? VendorEmail { get; set; }
    public string? VendorMobile { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal UtilizedValue { get; set; }
    public decimal BalanceValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string? CreatedByName { get; set; }

    // Workflow / approval fields
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public int ApprovalRequestHeaderId { get; set; }
    public byte IsEdit { get; set; }

    public List<GetContractPOMasterPendingDto> Lines { get; set; } = new();
}
