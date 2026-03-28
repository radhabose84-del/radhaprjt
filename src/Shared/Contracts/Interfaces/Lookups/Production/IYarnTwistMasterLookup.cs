using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production;

public interface IYarnTwistMasterLookup
{
    Task<IReadOnlyList<YarnTwistMasterLookupDto>> GetAllYarnTwistMasterAsync();
    Task<IReadOnlyList<YarnTwistMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
}
