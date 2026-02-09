using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Projects;

namespace Contracts.Interfaces.Lookups.Projects
{
    public interface IProjectLookup
    {
        Task<IReadOnlyList<ProjectLookupDto>> GetByIdsAsync(IEnumerable<int> projectIds, CancellationToken ct = default);
    }
}
