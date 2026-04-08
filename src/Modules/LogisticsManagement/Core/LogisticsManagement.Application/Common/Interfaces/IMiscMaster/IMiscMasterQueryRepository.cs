using LogisticsManagement.Application.MiscMaster.Dto;

namespace LogisticsManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<MiscMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<MiscMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<MiscMasterLookupDto>> AutocompleteAsync(string term, string? miscTypeCode, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscTypeExistsAsync(int miscTypeId);
    }
}
