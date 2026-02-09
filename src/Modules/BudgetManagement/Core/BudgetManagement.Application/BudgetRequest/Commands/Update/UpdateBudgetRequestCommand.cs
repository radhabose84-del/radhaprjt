using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Commands.Update;

public class UpdateBudgetRequestCommand : IRequest
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public int CurrencyId { get; set; }
    public int RequestTypeId { get; set; }
    public int? RequestById { get; set; }
    public int? RequestMonthId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BudgetGroupId { get; set; }
    public int? ProjectId { get; set; }
    public int? WBSId { get; set; }
    public decimal RequestAmount { get; set; }
    public string? Remarks { get; set; }
    public string? ImagePath { get; set; }
    public int EditFlag { get; set; }    
    public int FinancialYearId { get; set; }
}

