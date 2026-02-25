using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesGroup
{
    public interface ISalesGroupQueryRepository
    {
        Task<(List<SalesGroupDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesGroupDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string salesGroupName, int salesOfficeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesOfficeExistsAsync(int salesOfficeId);
        Task<bool> ProductCategoryExistsAsync(int categoryId, CancellationToken ct = default);
    }
}
