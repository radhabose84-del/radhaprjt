using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;

namespace PurchaseManagement.Application.Common.Interfaces.PriceMaster
{
    public interface IPriceMasterQueryRepository
    {
        Task<PriceMasterUpdateDto?> GetForEditAsync(int id, CancellationToken ct);
        Task<PagedResult<PriceMasterGetAllDto>> GetAllAsync(int? page, int? size, string? searchTerm, int? itemId, decimal? qtyFrom, decimal? qtyTo,int? statusId,bool expiredList,   CancellationToken ct);
        Task<PriceMasterGetAllDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<(List<PriceMasterPendingGroupDto>, int)> GetPriceMasterPendingAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<UnitPriceDto>> GetUnitPriceByQtyANDItemId(IEnumerable<ItemQtyDto> items);
    }
}
