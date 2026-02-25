using Contracts.Dtos.Lookups.Projects;

namespace Contracts.Interfaces.Lookups.Projects
{
    public interface IProjectWbsLookup
    {
        Task<IReadOnlyList<ProjectWbsLookupDto>> GetByIdsAsync(IEnumerable<int> wbsIds, CancellationToken ct = default);
    }
}
