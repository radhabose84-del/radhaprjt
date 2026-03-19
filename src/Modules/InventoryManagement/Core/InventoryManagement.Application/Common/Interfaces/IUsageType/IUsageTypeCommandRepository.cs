namespace InventoryManagement.Application.Common.Interfaces.IUsageType
{
    public interface IUsageTypeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.UsageType entity);
        Task<int> UpdateAsync(Domain.Entities.UsageType entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
