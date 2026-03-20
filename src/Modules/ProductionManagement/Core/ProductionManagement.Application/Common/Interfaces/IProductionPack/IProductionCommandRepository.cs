using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Application.Common.Interfaces.IProductionPack
{
    public interface IProductionCommandRepository
    {
        Task<int> CreateAsync(ProductionPackHeader entity, int typeId);
        Task<int> UpdateAsync(ProductionPackHeader entity);
    }
}
