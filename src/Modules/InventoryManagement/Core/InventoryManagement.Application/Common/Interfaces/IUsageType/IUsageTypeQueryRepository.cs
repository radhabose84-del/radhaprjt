using InventoryManagement.Application.UsageType.Dto;

namespace InventoryManagement.Application.Common.Interfaces.IUsageType
{
    public interface IUsageTypeQueryRepository
    {
        Task<(List<UsageTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<UsageTypeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<UsageTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AlreadyExistsAsync(string usageTypeCode, int? id = null);
    }
}
