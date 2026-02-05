namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IUnitOfWork
    {
        //Task BeginTransactionAsync(CancellationToken ct = default);
        //Task<int> SaveChangesAsync(CancellationToken ct = default);
        //Task CommitAsync(CancellationToken ct = default);
        //Task RollbackAsync(CancellationToken ct = default);
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
        Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default);
    }
}
