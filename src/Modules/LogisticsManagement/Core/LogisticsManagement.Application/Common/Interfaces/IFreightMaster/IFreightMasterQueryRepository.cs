using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.Application.Common.Interfaces.IFreightMaster
{
    public interface IFreightMasterQueryRepository
    {
        Task<(List<FreightMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<FreightMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<FreightMasterLookupDto>> AutocompleteAsync(string term, int? moduleId, CancellationToken ct);
        Task<bool> CompositeKeyExistsAsync(int freightModeId, int rateMethodId, int moduleId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> IsValidModeMethodCombinationAsync(int freightModeId, int rateMethodId);
    }
}
