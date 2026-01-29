using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUserRoleAllocation;
using Core.Domain.Entities;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationQueryRepository
{
    public class UserRoleAllocationQueryRepository : IUserRoleAllocationQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserRoleAllocationQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<Core.Domain.Entities.UserRoleAllocation>> GetAllAsync()
        {
            const string query = @"
                SELECT 
                    ura.Id AS UserRoleAllocationId,
                    ura.UserId,
                    ura.UserRoleId,
                    ur.RoleName
                FROM 
                    AppSecurity.UserRoleAllocation ura
                INNER JOIN 
                    AppSecurity.UserRole ur
                ON 
                    ura.UserRoleId = ur.UserRoleId";

            var allocations = await _dbConnection.QueryAsync<Core.Domain.Entities.UserRoleAllocation, UserRole, Core.Domain.Entities.UserRoleAllocation>(
                query,
                (ura, ur) =>
                {
                    ura.UserRole = ur;
                    return ura;
                },
                splitOn: "UserRoleId"
            );

            return allocations.ToList();
        }

        public async Task<Core.Domain.Entities.UserRoleAllocation?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT 
                    ura.Id AS UserRoleAllocationId,
                    ura.UserId,
                    ura.UserRoleId,
                    ur.RoleName
                FROM 
                    AppSecurity.UserRoleAllocation ura
                INNER JOIN 
                    AppSecurity.UserRole ur
                ON 
                    ura.UserRoleId = ur.UserRoleId
                WHERE 
                    ura.Id = @Id";

            var allocations = await _dbConnection.QueryAsync<Core.Domain.Entities.UserRoleAllocation, UserRole, Core.Domain.Entities.UserRoleAllocation>(
                query,
                (ura, ur) =>
                {
                    ura.UserRole = ur;
                    return ura;
                },
                param: new { Id = id },
                splitOn: "UserRoleId"
            );

            return allocations.FirstOrDefault();
        }

        public async Task<List<Core.Domain.Entities.UserRoleAllocation>> GetByUserIdAsync(int userId)
        {
            const string query = @"
                SELECT 
                    ura.Id AS UserRoleAllocationId,
                    ura.UserId,
                    ura.UserRoleId,
                    ur.RoleName
                FROM 
                    AppSecurity.UserRoleAllocation ura
                INNER JOIN 
                    AppSecurity.UserRole ur
                ON 
                    ura.UserRoleId = ur.UserRoleId
                WHERE 
                    ura.UserId = @UserId";

            var allocations = await _dbConnection.QueryAsync<Core.Domain.Entities.UserRoleAllocation, UserRole, Core.Domain.Entities.UserRoleAllocation>(
                query,
                (ura, ur) =>
                {
                    ura.UserRole = ur;
                    return ura;
                },
                param: new { UserId = userId },
                splitOn: "UserRoleId"
            );

            return allocations.ToList();
        }
    }
}
