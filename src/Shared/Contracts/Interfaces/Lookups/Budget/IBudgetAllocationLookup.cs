using System.Data.Common;
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

        /// <summary>
        /// Shared Transaction overload — executes the Budget UPDATE on the caller's
        /// existing ADO.NET connection and transaction so that the Budget deduction
        /// is atomically committed or rolled back together with the caller's writes
        /// (e.g. Purchase Order creation).
        /// Use this when the caller has already opened a DbTransaction via
        /// BeginTransactionAsync() and wants the Budget write inside the same
        /// SQL transaction.
        /// </summary>
        Task<bool> ApplyRemainingBalanceDeltaAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            decimal deltaAmount,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            DbConnection connection,
            DbTransaction transaction,
            CancellationToken ct = default);
    }
}
