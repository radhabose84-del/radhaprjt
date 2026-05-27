namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class PurchaseReturnHeaderDto
{
    public int Id { get; set; }
    public string RtvNumber { get; set; } = string.Empty;
    public DateOnly RtvDate { get; set; }
    public int UnitId { get; set; }

    public int VendorId { get; set; }
    public string? VendorName { get; set; }

    public int PoId { get; set; }
    public string? PoNumber { get; set; }

    public int GrnHeaderId { get; set; }
    public string? GrnNo { get; set; }

    public int ReturnTypeId { get; set; }
    public string? ReturnTypeCode { get; set; }
    public string? ReturnTypeDescription { get; set; }

    public int ReturnReasonId { get; set; }
    public string? ReturnReasonCode { get; set; }
    public string? ReturnReasonDescription { get; set; }

    public int ReturnActionId { get; set; }
    public string? ReturnActionCode { get; set; }

    public bool IsReplacementRequired { get; set; }
    public bool IsDebitNoteRequired { get; set; }
    public bool IsQcVerified { get; set; }

    public string? Remarks { get; set; }

    public int StatusId { get; set; }
    public string? StatusCode { get; set; }

    public int? ApprovalRequestId { get; set; }
    public int? ReplacementStatusId { get; set; }
    public string? ReplacementStatusCode { get; set; }
    public DateTimeOffset? ReplacementClosedDate { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }

    public List<PurchaseReturnDetailDto> Details { get; set; } = new();
}
