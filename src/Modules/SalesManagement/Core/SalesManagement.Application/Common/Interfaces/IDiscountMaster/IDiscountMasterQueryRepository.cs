using SalesManagement.Application.DiscountMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IDiscountMaster
{
    public interface IDiscountMasterQueryRepository
    {
        Task<(List<DiscountMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<DiscountMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<DiscountMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string discountName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> SalesGroupExistsAsync(int id);
    }
}
