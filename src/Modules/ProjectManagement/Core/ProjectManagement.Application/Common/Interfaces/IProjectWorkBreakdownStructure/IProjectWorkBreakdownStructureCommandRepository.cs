namespace ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure
{
    public interface IProjectWorkBreakdownStructureCommandRepository
  {
    Task<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure?> GetByIdAsync(int id);
    Task<ProjectManagement.Domain.Entities.ProjectMaster?> GetProjectAsync(int projectId);
    Task<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure> AddAsync(ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure entity);
    Task UpdateAsync(ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure entity);
    Task<bool> DeleteAsync(int id);

    Task<bool> NameExistsAsync(int projectId, string name);
    Task<bool> NameExistsAsync(int projectId, string name, int excludeId);
    
    }
}