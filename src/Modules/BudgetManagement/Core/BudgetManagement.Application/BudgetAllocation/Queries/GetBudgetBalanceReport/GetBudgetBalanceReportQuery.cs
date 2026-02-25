using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport
{
    public class GetBudgetBalanceReportQuery  : IRequest<List<BudgetBalanceReportDto>>
    {
         public int FinancialYearId { get; set; }
    }
}