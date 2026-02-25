namespace ProjectManagement.Application.Common.Interfaces.IProjectMaster
{
    public interface IProjectMasterCommandRepository
    {
        Task<ProjectManagement.Domain.Entities.ProjectMaster> CreateAsync(ProjectManagement.Domain.Entities.ProjectMaster entity, CancellationToken ct = default);
        Task<ProjectManagement.Domain.Entities.ProjectMaster?> GetByIdAsync(int id, CancellationToken ct = default);
        Task UpdateAsync(ProjectManagement.Domain.Entities.ProjectMaster projectMaster, CancellationToken ct = default);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);

       // Task<bool> RollbackProjectStatusAsync(int id, CancellationToken ct = default);
        Task<bool> UpdateProjectApprovalStatusAsync(int projectId, int statusId, CancellationToken ct = default);
        Task<bool> RollbackProjectStatusAsync(int projectId, CancellationToken ct = default);
    }
}