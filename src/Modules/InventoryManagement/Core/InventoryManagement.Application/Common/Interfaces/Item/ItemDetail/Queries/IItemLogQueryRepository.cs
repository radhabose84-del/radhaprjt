using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;

public interface IItemLogQueryRepository
{
    Task<(List<ItemLogDto> Items, int TotalCount)> GetAllAsync(
        ItemLogFilter filter,
        CancellationToken ct = default);

    Task<ItemLogDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<(List<ItemLogDto> Items, int TotalCount)> GetForEntityAsync(
        string entityName, int entityId, int? page, int? size, CancellationToken ct = default);
}

