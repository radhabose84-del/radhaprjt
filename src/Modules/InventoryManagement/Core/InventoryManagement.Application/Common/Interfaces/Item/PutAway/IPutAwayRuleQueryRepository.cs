using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;

namespace InventoryManagement.Application.Common.Interfaces.Item.PutAway
{
    public interface IPutAwayRuleQueryRepository
    {
        Task<(IEnumerable<PutAwayRuleListDto> rows, int total)> GetAllAsync(int page, int size, string? search, CancellationToken ct = default);
        Task<PutAwayRuleDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<GetPutAwayRuleItemIdDto?>> GetPutAwayRuleDetailsAsync(List<int> itemids,List<int> warehouseIds);
        Task<List<GetPutAwayRuleItemIdDto?>> GetPutAwayRuleWarehouseDetailsAsync(List<int> itemids, List<int> warehouseIds);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}