using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db) => _db = db;

   /*      public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            _tx = await _db.Database.BeginTransactionAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_tx != null) await _tx.CommitAsync(ct);
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx != null) await _tx.RollbackAsync(ct);
        }

        public void Dispose()
        {
            _tx?.Dispose();
        } */

        public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken ct = default)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    await work(ct);
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }


        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> work,
            CancellationToken ct = default)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    var result = await work(ct);
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return result;
                }
            catch (DbUpdateException ex) when (ex.InnerException != null)
                {
                    await tx.RollbackAsync(ct);
                    // Bubble up the *SQL* error so you see FK/UK/NOT NULL details
                    throw new InvalidOperationException(
                        $"DB write failed: {ex.InnerException.Message}", ex);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}
