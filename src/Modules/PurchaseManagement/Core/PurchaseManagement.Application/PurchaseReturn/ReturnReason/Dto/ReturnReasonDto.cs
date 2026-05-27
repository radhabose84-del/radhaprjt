namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

public class ReturnReasonDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReturnTypeId { get; set; }
    public string? ReturnTypeName { get; set; }
    public bool? IsReplacementOverride { get; set; }
    public bool? IsDebitNoteOverride { get; set; }
    public bool? IsQcMandatoryOverride { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class ReturnReasonLookupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReturnTypeId { get; set; }
}
