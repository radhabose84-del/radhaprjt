namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

public class ReturnTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? InventoryImpactId { get; set; }
    public string? InventoryImpactName { get; set; }
    public int? FinanceImpactId { get; set; }
    public string? FinanceImpactName { get; set; }
    public bool IsReplacementApplicable { get; set; }
    public bool IsQcMandatory { get; set; }
    public string? ApprovalRoleCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class ReturnTypeLookupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsReplacementApplicable { get; set; }
    public bool IsQcMandatory { get; set; }
}
