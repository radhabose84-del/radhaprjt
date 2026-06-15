using System.Data;
using Dapper;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;

namespace UserManagement.Infrastructure.Repositories.AccessPolicy
{
    public class AccessPolicyQueryRepository : IAccessPolicyQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public AccessPolicyQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<AccessPolicyDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM [AppSecurity].[AccessPolicy]
                WHERE IsDeleted = 0
                {(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (PolicyCode LIKE @Search OR PolicyName LIKE @Search OR EntityName LIKE @Search)")};

                SELECT Id, PolicyCode, PolicyName, EntityName, FieldName,
                       IsActive, IsDeleted, CreatedBy, CreatedAt, CreatedByName, ModifiedBy, ModifiedAt
                FROM [AppSecurity].[AccessPolicy]
                WHERE IsDeleted = 0
                {(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (PolicyCode LIKE @Search OR PolicyName LIKE @Search OR EntityName LIKE @Search)")}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var data       = (await multi.ReadAsync<AccessPolicyDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (data, totalCount);
        }

        public async Task<AccessPolicyDto?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT Id, PolicyCode, PolicyName, EntityName, FieldName,
                       IsActive, IsDeleted, CreatedBy, CreatedAt, CreatedByName, ModifiedBy, ModifiedAt
                FROM [AppSecurity].[AccessPolicy]
                WHERE Id = @Id AND IsDeleted = 0
                """;

            return await _dbConnection.QueryFirstOrDefaultAsync<AccessPolicyDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<AccessPolicyDto>> AutocompleteAsync(
            string term, CancellationToken ct)
        {
            const string sql = """
                SELECT Id, PolicyCode, PolicyName, EntityName, FieldName
                FROM [AppSecurity].[AccessPolicy]
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (PolicyCode LIKE @Term OR PolicyName LIKE @Term)
                ORDER BY PolicyName ASC
                """;

            var result = await _dbConnection.QueryAsync<AccessPolicyDto>(
                sql, new { Term = $"%{term}%" });

            return result.ToList().AsReadOnly();
        }

        public async Task<bool> AlreadyExistsAsync(string policyCode, int? excludeId = null)
        {
            var sql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[AccessPolicy]
                WHERE PolicyCode = @PolicyCode AND IsDeleted = 0
                """;

            var parameters = new DynamicParameters(new { PolicyCode = policyCode });

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeId.Value);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[AccessPolicy]
                WHERE Id = @Id AND IsDeleted = 0
                """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<List<RoleAccessPolicyDto>> GetRoleAccessPoliciesAsync(
            int accessPolicyId, int? roleId = null)
        {
            var sql = """
                SELECT rap.Id, rap.AccessPolicyId, rap.RoleId, rap.ValueId,
                       ap.PolicyCode, ap.PolicyName,
                       ur.RoleName
                FROM [AppSecurity].[RoleAccessPolicy] rap
                INNER JOIN [AppSecurity].[AccessPolicy] ap
                    ON ap.Id = rap.AccessPolicyId AND ap.IsDeleted = 0
                LEFT JOIN [AppSecurity].[UserRole] ur
                    ON ur.Id = rap.RoleId AND ur.IsDeleted = 0
                WHERE rap.AccessPolicyId = @AccessPolicyId
                """;

            var parameters = new DynamicParameters(new { AccessPolicyId = accessPolicyId });

            if (roleId.HasValue)
            {
                sql += " AND rap.RoleId = @RoleId";
                parameters.Add("RoleId", roleId.Value);
            }

            var result = await _dbConnection.QueryAsync<RoleAccessPolicyDto>(sql, parameters);
            return result.ToList();
        }

        public async Task<bool> RoleValueAssignmentExistsAsync(
            int accessPolicyId, int roleId, int valueId, int? excludeId = null)
        {
            var sql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[RoleAccessPolicy]
                WHERE AccessPolicyId = @AccessPolicyId
                  AND RoleId  = @RoleId
                  AND ValueId = @ValueId
                """;

            var parameters = new DynamicParameters(new
            {
                AccessPolicyId = accessPolicyId,
                RoleId         = roleId,
                ValueId        = valueId
            });

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeId.Value);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<bool> RoleAccessPolicyNotFoundAsync(int id)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[RoleAccessPolicy]
                WHERE Id = @Id
                """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> UserRoleExistsAsync(int roleId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[UserRole]
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = roleId });
            return count > 0;
        }
    }
}
