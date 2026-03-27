using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.QualityMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IQualityMaster
{
    public interface IQualityMasterQueryRepository
    {
        Task<(List<QualityMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<QualityMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<QualityMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> QualityNameExistsAsync(string qualityName, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
