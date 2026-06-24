namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod
{
    public interface IAccountingPeriodCommandRepository
    {
        Task<int> CreateAsync(FinanceManagement.Domain.Entities.AccountingPeriod entity);

        // Updates mutable fields only (PeriodNo + FinancialYearId are immutable, like a code).
        Task<int> UpdateAsync(FinanceManagement.Domain.Entities.AccountingPeriod entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
