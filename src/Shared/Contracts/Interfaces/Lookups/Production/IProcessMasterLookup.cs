using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production;

public interface IProcessMasterLookup
{
    Task<IReadOnlyList<ProcessMasterLookupDto>> GetAllProcessMasterAsync();
    Task<IReadOnlyList<ProcessMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
}
