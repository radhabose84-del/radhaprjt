using SalesManagement.Application.StoTypeMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IStoTypeMaster
{
    public interface IStoTypeMasterQueryRepository
    {
        Task<(List<StoTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<StoTypeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<StoTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string stoTypeCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MovementTypeExistsAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsStoTypeMasterLinkedAsync(int id);
    }
}
