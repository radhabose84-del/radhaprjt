using MediatR;

namespace InventoryManagement.Application.Budget.Queries.GetBudgetLogs
{
    public class GetBudgetLogsQuery : IRequest<List<BudgetLogDto>>
    {
        public int? BudgetId { get; set; }
        public int? BudgetDetailId { get; set; }
    }
}
