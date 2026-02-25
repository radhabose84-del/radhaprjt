using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<MiscMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? miscTypeId = null);
        Task<MiscMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MiscMasterLookupDto>> AutocompleteAsync(string term, int? miscTypeId, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscTypeExistsAsync(int miscTypeId);
    }
}
