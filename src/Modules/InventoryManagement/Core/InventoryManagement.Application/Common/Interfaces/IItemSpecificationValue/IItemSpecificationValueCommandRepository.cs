
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue
{
    public interface IItemSpecificationValueCommandRepository
    {
        Task<int> CreateAsync(DomainEntities.ItemSpecificationValue entity);
        Task<int> UpdateAsync(DomainEntities.ItemSpecificationValue entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    }
}
