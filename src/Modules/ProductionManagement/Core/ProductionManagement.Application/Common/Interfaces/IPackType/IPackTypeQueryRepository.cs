using ProductionManagement.Application.PackType.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IPackType
{
    public interface IPackTypeQueryRepository
    {
        Task<(List<PackTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<PackTypeDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<PackTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string packTypeCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
