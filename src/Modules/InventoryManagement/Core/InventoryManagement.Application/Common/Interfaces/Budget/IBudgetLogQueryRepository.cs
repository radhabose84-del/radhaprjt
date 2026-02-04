using InventoryManagement.Application.Budget.Queries.GetBudgetLogs;

namespace InventoryManagement.Application.Common.Interfaces.Budget
{
    public interface IBudgetLogQueryRepository
    {
        Task<List<BudgetLogDto>> GetLogsAsync(int? budgetId, int? budgetDetailId);
    }
}
