using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.CertificationMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.ICertificationMaster
{
    public interface ICertificationMasterQueryRepository
    {
        Task<(List<CertificationMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<CertificationMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CertificationMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> CertificationNameExistsAsync(string certificationName, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
