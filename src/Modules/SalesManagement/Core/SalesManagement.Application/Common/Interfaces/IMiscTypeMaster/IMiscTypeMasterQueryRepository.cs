#nullable disable
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<MiscTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm);
        Task<MiscTypeMasterDto> GetByIdAsync(int id);
        Task<IReadOnlyList<MiscTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null);
        Task<bool> NotFoundAsync(int id);
    }
}
