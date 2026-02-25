using PurchaseManagement.Application.MRS.Queries.GetMrsEntry;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntryById;
using PurchaseManagement.Application.MRS.Queries.GetMrsPending;
using PurchaseManagement.Application.MRS.Queries.GetStockItemBased;

namespace PurchaseManagement.Application.Common.Interfaces.IMRS
{
    public interface IMrsEntryQueryRepository
    {
        Task<(List<GetMrsEntryDto>, int)> GetMrsEntryDetails(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? fromDate, DateTimeOffset? toDate);
        Task<GetMrsEntryByIdDto> GetMrsDetailsById(int id);
        Task<List<GetStockItemDto>> GetStockDetails(int itemId, int warehouseId);
        Task<(List<MrsPendingDto>, int)> GetPendingMrsDetailsAsync(int PageNumber, int PageSize, string? SearchTerm);
    }
}