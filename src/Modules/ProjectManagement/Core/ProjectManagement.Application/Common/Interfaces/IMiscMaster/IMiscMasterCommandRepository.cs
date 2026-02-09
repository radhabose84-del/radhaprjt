using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterCommandRepository
    {
        Task<ProjectManagement.Domain.Entities.MiscMaster> CreateAsync(ProjectManagement.Domain.Entities.MiscMaster miscMaster);

        Task<int> GetMaxSortOrderAsync();

        Task<bool> UpdateAsync(int id, ProjectManagement.Domain.Entities.MiscMaster miscMaster);

        Task<bool> DeleteAsync(int id, ProjectManagement.Domain.Entities.MiscMaster miscMaster);  
        Task<Dictionary<int, Domain.Entities.MiscMaster>> GetManyByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}