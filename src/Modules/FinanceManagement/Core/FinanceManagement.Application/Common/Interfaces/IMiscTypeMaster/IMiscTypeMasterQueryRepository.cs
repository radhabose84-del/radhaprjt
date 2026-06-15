using FinanceManagement.Application.MiscTypeMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<MiscTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<MiscTypeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MiscTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsMiscTypeMasterLinkedAsync(int id);
    }
}
