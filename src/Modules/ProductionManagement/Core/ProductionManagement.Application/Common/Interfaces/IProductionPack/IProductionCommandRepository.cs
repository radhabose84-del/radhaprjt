using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Application.Common.Interfaces.IProductionPack
{
    public interface IProductionCommandRepository
    {
        Task<int> CreateAsync(ProductionPackEntry entity, int typeId);
        Task<int> UpdateAsync(ProductionPackEntry entity);
    }
}
