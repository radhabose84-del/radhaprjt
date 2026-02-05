using InventoryManagement.Domain.Entities.Item.PutAway;

namespace InventoryManagement.Application.Common.Interfaces.Item.PutAway
{
    public interface IPutAwayRuleCommandRepository
    {
        Task<bool> ExistsScopeAsync(int unitId, int warehouseId, int itemGroupId, int itemCategoryId, int? itemId, int? excludeId = null, CancellationToken ct = default);
        Task AddAsync(PutAwayRule entity, CancellationToken ct = default);
        Task<PutAwayRule?> GetByIdAsync(int id, bool track, CancellationToken ct = default);
        Task SaveAsync(CancellationToken ct = default);
        Task SoftDeleteAsync(PutAwayRule entity, CancellationToken ct = default);
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);    
        
    }
}