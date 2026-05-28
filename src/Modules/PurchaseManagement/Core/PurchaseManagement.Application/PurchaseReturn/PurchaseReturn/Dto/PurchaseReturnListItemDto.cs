namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class PurchaseReturnListItemDto
{
    public int Id { get; set; }
    public string RtvNumber { get; set; } = string.Empty;
    public DateOnly RtvDate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int PoId { get; set; }
    public string? PoNumber { get; set; }
    public int GrnHeaderId { get; set; }
    public string? GrnNo { get; set; }
    public int ReturnTypeId { get; set; }
    public string? ReturnTypeCode { get; set; }
    public int ReturnReasonId { get; set; }
    public string? ReturnReasonCode { get; set; }
    public int StatusId { get; set; }
    public string? StatusCode { get; set; }
    public bool IsReplacementRequired { get; set; }
    public bool IsDebitNoteRequired { get; set; }
    public bool IsActive { get; set; }
}
