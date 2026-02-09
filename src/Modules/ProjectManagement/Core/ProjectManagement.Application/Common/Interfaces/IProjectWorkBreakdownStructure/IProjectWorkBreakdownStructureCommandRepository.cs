using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IProjectWorkBreakdownStructure
{
  public interface IProjectWorkBreakdownStructureCommandRepository
  {
    Task<Core.Domain.Entities.ProjectWorkBreakdownStructure?> GetByIdAsync(int id);
    Task<Core.Domain.Entities.ProjectMaster?> GetProjectAsync(int projectId);
    Task<Core.Domain.Entities.ProjectWorkBreakdownStructure> AddAsync(Core.Domain.Entities.ProjectWorkBreakdownStructure entity);
    Task UpdateAsync(Core.Domain.Entities.ProjectWorkBreakdownStructure entity);
    Task<bool> DeleteAsync(int id);

    Task<bool> NameExistsAsync(int projectId, string name);
    Task<bool> NameExistsAsync(int projectId, string name, int excludeId);
    
    }
}