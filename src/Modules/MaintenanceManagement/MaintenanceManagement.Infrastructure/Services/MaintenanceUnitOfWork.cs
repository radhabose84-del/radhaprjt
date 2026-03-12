using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MaintenanceManagement.Infrastructure.Services;

/// <summary>
/// Wraps ApplicationDbContext's SQL transaction.
/// Because OutboxRepository and all command repositories share the same scoped
/// ApplicationDbContext, BeginTransactionAsync makes every SaveChangesAsync call
/// within that scope write inside the open transaction — nothing is visible to
/// other connections until CommitAsync is called.
///
/// Crash safety:
///   • Exception before CommitAsync  → RollbackAsync undoes all writes (domain + outbox).
///   • Exception after CommitAsync   → outbox row is committed; BSOFT.Worker picks it up
///     on next poll and publishes to RabbitMQ (at-least-once guarantee).
/// </summary>
internal sealed class MaintenanceUnitOfWork : IMaintenanceUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public MaintenanceUnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        // Flush any change-tracker entries not yet saved (e.g. from ScheduleWithoutSaveAsync).
        await _dbContext.SaveChangesAsync(ct);
        await _transaction!.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
