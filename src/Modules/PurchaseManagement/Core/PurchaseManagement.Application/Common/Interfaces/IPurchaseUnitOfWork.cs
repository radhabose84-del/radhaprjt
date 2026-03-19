namespace PurchaseManagement.Application.Common.Interfaces;

/// <summary>
/// Manages SQL transaction scope for atomic domain + outbox operations.
/// Both domain writes (EF Core) and outbox inserts share the same SQL transaction,
/// guaranteeing that either both commit or both rollback on any failure.
/// </summary>
public interface IPurchaseUnitOfWork
{
    /// <summary>Opens a SQL transaction. Must be called before any write operations.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Flushes any pending change-tracker entries (e.g. ScheduleWithoutSaveAsync)
    /// then commits the SQL transaction.
    /// </summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>Rolls back all writes made since BeginTransactionAsync. Safe to call when no transaction is open.</summary>
    Task RollbackAsync(CancellationToken ct = default);
}
