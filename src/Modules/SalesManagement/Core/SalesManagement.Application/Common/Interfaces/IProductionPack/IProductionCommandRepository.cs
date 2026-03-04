using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IProductionPack
{
    public interface IProductionCommandRepository
    {
        Task<string> GenerateNextPackNoAsync(int warehouseId, int binId, CancellationToken ct = default);
        Task<int> CreateAsync(ProductionPackHeader entity);
        Task<int> UpdateAsync(ProductionPackHeader entity);
    }
}
