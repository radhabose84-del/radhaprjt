using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production;

public interface IQualityMasterLookup
{
    Task<IReadOnlyList<QualityMasterLookupDto>> GetAllQualityMasterAsync();
    Task<IReadOnlyList<QualityMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
}
