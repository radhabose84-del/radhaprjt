using Contracts.Dtos.Budget;

namespace Contracts.Interfaces.External.IBudget
{
    public interface IBudgetGroupGrpcClient
    {
        Task<BudgetGroupMasterDto?> GetByIdAsync(int id);
    }
}
