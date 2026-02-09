namespace BudgetManagement.Application.BudgetRequest;

public class BudgetRequestDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public int UnitId { get; set; }
    public int CurrencyId { get; set; }
    public int RequestTypeId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int StatusId { get; set; }
    public decimal RequestAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ImagePath { get; set; }
    public string? ImageUrl { get; set; }
}

public class BudgetRequestListItemDto
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public int UnitId { get; set; }
    public int FinancialYearId { get; set; }
    public int CurrencyId { get; set; }
    public int RequestTypeId { get; set; }
    public int RequestMonthId { get; set; }
    public int RequestById { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int? WBSId { get; set; }
    public string? WBSName { get; set; }
    public int StatusId { get; set; }
    public decimal RequestAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ImagePath { get; set; }
    public int? RevisionNumber { get; set; }

    // Audit fields
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public int IsActive { get; set; }

    // Enriched fields
    public string UnitName { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public string FinancialYearName { get; set; } = string.Empty;
    public string RequestTypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string RequestByName { get; set; } = string.Empty;
    public string RequestMonthName { get; set; } = string.Empty;
    public int EditFlag { get; set; }
    public string? EditReason { get; set; }
    public string BudgetGroupName { get; set; } = string.Empty;
    
}

public class BudgetRequestListRaw
{
    public int Id { get; set; }
    public string? RequestCode { get; set; }
    public int UnitId { get; set; }
    public int FinancialYearId { get; set; }
    public int CurrencyId { get; set; }
    public int RequestTypeId { get; set; }
    public int RequestById { get; set; }
    public int RequestMonthId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public int StatusId { get; set; }
    public decimal RequestAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ImagePath { get; set; }
    public int? RevisionNumber { get; set; }

    // Audit
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public int IsActive { get; set; }

    // joined fields
    public string RequestTypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string RequestByName { get; set; } = string.Empty;
    public string RequestMonthName { get; set; } = string.Empty;
    public string BudgetGroupName { get; set; }= string.Empty;
}

