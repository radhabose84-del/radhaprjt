using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.RoleItemGroupMapping
{
    public class RoleItemGroupMappingQueryRepository : IRoleItemGroupMappingQueryRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDbConnection _dbConnection;

        public RoleItemGroupMappingQueryRepository(
            ApplicationDbContext dbContext,
            IDbConnection dbConnection)
        {
            _dbContext = dbContext;
            _dbConnection = dbConnection;
        }

        public async Task<Domain.Entities.RoleItemGroupMapping?> GetByIdAsync(int id)
        {
            return await _dbContext.RoleItemGroupMapping
                .Include(x => x.UserRole)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id
                    && x.IsDeleted == Domain.Enums.Common.Enums.IsDelete.NotDeleted);
        }

        public async Task<(List<Domain.Entities.RoleItemGroupMapping>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _dbContext.RoleItemGroupMapping
                .Include(x => x.UserRole)
                .AsNoTracking()
                .Where(x => x.IsDeleted == Domain.Enums.Common.Enums.IsDelete.NotDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x =>
                    (x.UserRole != null && x.UserRole.RoleName != null
                        && x.UserRole.RoleName.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<List<Domain.Entities.RoleItemGroupMapping>> GetByRoleIdAsync(int roleId)
        {
            return await _dbContext.RoleItemGroupMapping
                .Include(x => x.UserRole)
                .AsNoTracking()
                .Where(x => x.RoleId == roleId
                    && x.IsDeleted == Domain.Enums.Common.Enums.IsDelete.NotDeleted
                    && x.IsActive == Domain.Enums.Common.Enums.Status.Active)
                .OrderBy(x => x.ItemGroupId)
                .ToListAsync();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            return !await _dbContext.RoleItemGroupMapping
                .AnyAsync(x => x.Id == id
                    && x.IsDeleted == Domain.Enums.Common.Enums.IsDelete.NotDeleted);
        }

        public Task<bool> SoftDeleteValidation(int id)
        {
            // RoleItemGroupMapping is a leaf table — no dependents to check
            return Task.FromResult(false);
        }
    }
}
