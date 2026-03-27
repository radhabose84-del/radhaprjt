using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.CountGroup.Dto;

namespace ProductionManagement.Application.Common.Interfaces.ICountGroup
{
    public interface ICountGroupQueryRepository
    {
        Task<(List<CountGroupDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<CountGroupDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CountGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string countGroupCode, int? id = null);
        Task<bool> CountGroupNameExistsAsync(string countGroupName, int? id = null);
        Task<bool> NotFoundAsync(int id);
    }
}
