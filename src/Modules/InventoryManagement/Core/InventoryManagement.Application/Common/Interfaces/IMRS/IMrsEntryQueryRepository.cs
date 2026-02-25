using InventoryManagement.Application.MRS.Queries.GetMrsEntry;
using InventoryManagement.Application.MRS.Queries.GetMrsEntryById;
using InventoryManagement.Application.MRS.Queries.GetMrsPending;
using InventoryManagement.Application.MRS.Queries.GetStockItemBased;

namespace InventoryManagement.Application.Common.Interfaces.IMRS
{
    public interface IMrsEntryQueryRepository
    {
        Task<(List<GetMrsEntryDto>, int)> GetMrsEntryDetails(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? fromDate, DateTimeOffset? toDate);
        Task<GetMrsEntryByIdDto> GetMrsDetailsById(int id);
        Task<List<GetStockItemDto>> GetStockDetails(int itemId, int warehouseId);
        Task<(List<MrsPendingDto>, int)> GetPendingMrsDetailsAsync(int PageNumber, int PageSize, string? SearchTerm);
    }
}