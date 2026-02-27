using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster
{
    public interface ISalesItemPriceMasterQueryRepository
    {
        Task<(List<SalesItemPriceMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesItemPriceMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesItemPriceMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string priceCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);
        Task<bool> SalesSegmentExistsAsync(int salesSegmentId);
        Task<bool> PaymentTermExistsAsync(int paymentTermsId);
        Task<bool> CurrencyExistsAsync(int currencyId, CancellationToken ct = default);
        Task<bool> OverlapExistsAsync(int itemId, int salesSegmentId, int paymentTermsId,
            DateOnly validFrom, DateOnly validTo, int? excludeId = null);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
