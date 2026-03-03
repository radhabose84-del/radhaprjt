using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.Common.Interfaces.ICustomerVisit
{
    public interface ICustomerVisitQueryRepository
    {
        Task<(List<CustomerVisitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<CustomerVisitDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CustomerVisitLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> VisitTypeExistsAsync(int visitTypeId);
        Task<bool> MarketingOfficerExistsAsync(int marketingOfficerId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<string?> GetMarketingOfficerNameAsync(int marketingOfficerId);
        Task<string> GetImageFolderAsync();
    }
}
