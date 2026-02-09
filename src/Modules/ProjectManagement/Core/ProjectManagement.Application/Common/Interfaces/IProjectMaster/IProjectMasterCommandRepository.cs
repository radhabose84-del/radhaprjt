using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IProjectMaster
{
    public interface IProjectMasterCommandRepository
    {
        Task<Core.Domain.Entities.ProjectMaster> CreateAsync(Core.Domain.Entities.ProjectMaster entity, CancellationToken ct = default);
        Task<Core.Domain.Entities.ProjectMaster?> GetByIdAsync(int id, CancellationToken ct = default);
        Task UpdateAsync(Core.Domain.Entities.ProjectMaster projectMaster, CancellationToken ct = default);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);

       // Task<bool> RollbackProjectStatusAsync(int id, CancellationToken ct = default);
        Task<bool> UpdateProjectApprovalStatusAsync(int projectId, int statusId, CancellationToken ct = default);
        Task<bool> RollbackProjectStatusAsync(int projectId, CancellationToken ct = default);
    }
}