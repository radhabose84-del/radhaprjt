using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Infrastructure.Data;

namespace ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure
{
    public class ProjectWorkBreakdownStructureCommandRepository : IProjectWorkBreakdownStructureCommandRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectWorkBreakdownStructureCommandRepository(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure?> GetByIdAsync(int id)
        {
            // 👇 IMPORTANT: AsNoTracking + IsDeleted filter
            return await _context.ProjectWorkBreakdownStructures
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }

        // Implement GetProjectAsync to fetch the ProjectMaster by ProjectId
        public async Task<ProjectManagement.Domain.Entities.ProjectMaster?> GetProjectAsync(int projectId)
        {
            return await _context.ProjectMaster
                .FirstOrDefaultAsync(p => p.Id == projectId && p.IsDeleted == BaseEntity.IsDelete.NotDeleted);   // Adjust based on IsDeleted flag
        }

        public async Task<ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure> AddAsync(ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure entity)
        {
            _context.ProjectWorkBreakdownStructures.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure entity)
        {
            _context.ProjectWorkBreakdownStructures.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProjectWorkBreakdownStructures
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == ProjectManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            if (entity == null)
                return false;

            entity.IsDeleted = ProjectManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> NameExistsAsync(int projectId, string name)
        {
            return await _context.ProjectWorkBreakdownStructures
                .AnyAsync(x =>
                    x.ProjectId == projectId &&
                    x.WorkBreakdownStructureName == name &&
                    x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }
        
         public async Task<bool> NameExistsAsync(int projectId, string name, int excludeId)
        {
            return await _context.ProjectWorkBreakdownStructures
                .AnyAsync(x =>
                    x.ProjectId == projectId &&
                    x.Id != excludeId &&
                    x.WorkBreakdownStructureName == name &&
                    x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }
    }
}