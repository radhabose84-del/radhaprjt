using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryManagement.Infrastructure.Services
{
    internal sealed class InventoryUnitOfWork : IInventoryUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public InventoryUnitOfWork(ApplicationDbContext dbContext)
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
}
