using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IItemPriceMaster
{
    public interface IItemPriceMasterQueryRepository
    {
        Task<(List<ItemPriceMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ItemPriceMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ItemPriceMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string priceCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);
        Task<bool> SalesSegmentExistsAsync(int salesSegmentId);
        Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default);
        Task<bool> OverlapExistsAsync(int itemId, int salesSegmentId,
            DateOnly validFrom, DateOnly validTo, int? excludeId = null);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<List<ItemPriceMasterDto>> GetByItemAndDateAsync(int itemId, DateOnly date);
        Task<int> GetNextPriceCodeSerialAsync(string prefix);
        Task<bool> IsItemPriceMasterPendingAsync(int id);
        Task<List<ExMillRateDto>> GetExMillRateByPaymentTermAsync(int paymentTermId, int itemId, int? salesSegmentId = null);
    }
}
