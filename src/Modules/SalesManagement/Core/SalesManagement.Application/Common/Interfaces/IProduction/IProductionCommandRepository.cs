using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IProduction
{
    public interface IProductionCommandRepository
    {
        Task<string> GenerateNextPackNoAsync(int warehouseId, CancellationToken ct = default);
        Task<int> CreateAsync(ProductionPackHeader entity);
        Task<int> UpdateAsync(ProductionPackHeader entity);
    }
}
