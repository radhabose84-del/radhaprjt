using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Application.Common.Interfaces.IProductionPack
{
    public interface IProductionCommandRepository
    {
        Task<int> CreateAsync(ProductionPackDetail entity, int typeId);
        Task<int> UpdateAsync(ProductionPackDetail entity);
    }
}
