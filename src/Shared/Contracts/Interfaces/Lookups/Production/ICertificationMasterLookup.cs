using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production;

public interface ICertificationMasterLookup
{
    Task<IReadOnlyList<CertificationMasterLookupDto>> GetAllCertificationMasterAsync();
    Task<IReadOnlyList<CertificationMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
}
