using System.Data;
using UserManagement.Application.Common.Interfaces.IUserRoleAllocation;
using UserManagement.Domain.Entities;
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

        public async Task<List<UserManagement.Domain.Entities.UserRoleAllocation>> GetAllAsync()
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

            var allocations = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserRoleAllocation, UserRole, UserManagement.Domain.Entities.UserRoleAllocation>(
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

        public async Task<UserManagement.Domain.Entities.UserRoleAllocation?> GetByIdAsync(int id)
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

            var allocations = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserRoleAllocation, UserRole, UserManagement.Domain.Entities.UserRoleAllocation>(
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

        public async Task<List<UserManagement.Domain.Entities.UserRoleAllocation>> GetByUserIdAsync(int userId)
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

            var allocations = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.UserRoleAllocation, UserRole, UserManagement.Domain.Entities.UserRoleAllocation>(
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
