namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
public sealed class OpenRfqConflictDto
{
    public int RfqId { get; init; }
    public string RfqCode { get; init; } = default!;
    public int SupplierId { get; init; }
    public int ItemId { get; init; }
    public DateOnly? LastSubmitDate { get; init; }
}