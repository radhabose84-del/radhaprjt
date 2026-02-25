using Contracts.Dtos.Budget;

namespace Contracts.Interfaces.Lookups.Budget
{
    public interface IBudgetAllocationLookup
    {
        Task<RemainingBalanceWithPrevDto?> GetRemainingBalanceAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            CancellationToken ct = default);

        Task<bool> ApplyRemainingBalanceDeltaAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            decimal deltaAmount,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            CancellationToken ct = default);
    }
}
