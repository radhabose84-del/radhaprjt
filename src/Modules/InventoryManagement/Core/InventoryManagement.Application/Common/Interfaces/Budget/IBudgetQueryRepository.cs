using InventoryManagement.Application.Budget.Queries.GetAllBudgets;
using InventoryManagement.Application.Budget.Queries.GetBudgetById;

namespace InventoryManagement.Application.Common.Interfaces.Budget
{
    public interface IBudgetQueryRepository
    {
        Task<BudgetResponseDto?> GetBudgetByIdAsync(int budgetId);
         Task<List<BudgetListDto>> GetAllBudgetsAsync(int? fiscalYear);
    }
}
