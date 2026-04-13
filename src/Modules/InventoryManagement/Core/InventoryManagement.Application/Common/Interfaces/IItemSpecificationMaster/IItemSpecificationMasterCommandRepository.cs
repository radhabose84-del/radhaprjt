
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster
{
    public interface IItemSpecificationMasterCommandRepository
    {
        Task<int> CreateAsync(DomainEntities.ItemSpecificationMaster entity);
        Task<int> UpdateAsync(DomainEntities.ItemSpecificationMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    }
}
