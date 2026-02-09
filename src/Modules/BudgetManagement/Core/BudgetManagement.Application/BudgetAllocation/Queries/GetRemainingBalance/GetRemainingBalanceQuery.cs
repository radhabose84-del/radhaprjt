using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;

public class GetRemainingBalanceQuery : IRequest<RemainingBalanceWithPrevDto>
{
    public int BudgetGroupId { get; set; }
    public DateOnly? BudgetDate { get; set; }
    public int? RequestById { get; set; }
    public int? MonthId { get; set; }
    public int? ProjectId { get; set; }
    public int? WbsId { get; set; }
    public int? FinancialYearId { get; set; }    
}
 