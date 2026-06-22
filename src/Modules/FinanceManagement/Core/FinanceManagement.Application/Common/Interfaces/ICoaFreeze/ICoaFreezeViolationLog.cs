namespace FinanceManagement.Application.Common.Interfaces.ICoaFreeze
{
    // App-observed COA freeze violations (UI/API attempts blocked by the DB trigger), stored in MongoDB.
    // Raw-SQL attempts are blocked by the trigger but cannot be logged here (the rollback undoes any
    // in-transaction insert) — they land only in the SQL Server error log.
    public interface ICoaFreezeViolationLog
    {
        Task LogAsync(int companyId, int userId, string? operation, CancellationToken ct = default);

        // Blocked attempts since the freeze took effect (the "Blocked Attempts" card).
        Task<long> CountSinceAsync(int companyId, DateTimeOffset since, CancellationToken ct = default);
    }
}
