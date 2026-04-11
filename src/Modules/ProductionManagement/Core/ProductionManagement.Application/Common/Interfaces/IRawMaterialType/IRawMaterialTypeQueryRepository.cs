using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.RawMaterialType.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IRawMaterialType
{
    public interface IRawMaterialTypeQueryRepository
    {
        Task<(List<RawMaterialTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);

        Task<RawMaterialTypeDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<RawMaterialTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> AlreadyExistsAsync(string rawMaterialTypeCode, int? id = null);

        Task<bool> NameAlreadyExistsAsync(string rawMaterialTypeName, int? id = null);

        Task<bool> NotFoundAsync(int id);

        // Reserved for Rule #25 — currently always false (no dependent entity references RawMaterialTypeId yet).
        // When a future entity adds a FK to RawMaterialType, switch this to a real EXISTS query against
        // the dependent table(s) so the Update handler's deactivate guard and the Delete validator
        // can refuse to inactivate / delete a master that is still in use.
        Task<bool> IsRawMaterialTypeLinkedAsync(int id);
    }
}
