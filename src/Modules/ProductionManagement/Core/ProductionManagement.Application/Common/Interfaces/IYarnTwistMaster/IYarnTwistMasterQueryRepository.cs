using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.YarnTwistMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster
{
    public interface IYarnTwistMasterQueryRepository
    {
        Task<(List<YarnTwistMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<YarnTwistMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<YarnTwistMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> TwistNameExistsAsync(string twistName, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
