using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.ILedgerBalance
{
    public interface ILedgerBalanceQueryRepository
    {
        // Period ledger balances joined with GL account / account type / account group (same-module JOINs).
        // All filters optional except the company scope.
        Task<(List<LedgerBalanceDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, int? companyId,
            int? accountingPeriodId, int? financialYearId, int? glAccountId,
            int? accountTypeId, int? accountGroupId, int? costCentreId, string? searchTerm);
    }
}
