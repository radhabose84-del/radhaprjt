using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Services;

/// <summary>
/// Wraps ApplicationDbContext's SQL transaction so that domain writes (EF Core)
/// and outbox inserts share the same transaction — nothing is visible to other
/// connections until CommitAsync is called.
/// </summary>
internal sealed class PurchaseUnitOfWork : IPurchaseUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public PurchaseUnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
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
