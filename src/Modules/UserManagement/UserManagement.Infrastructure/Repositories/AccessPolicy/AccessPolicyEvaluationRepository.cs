using System.Data;
using Dapper;
using IEvalRepo = Contracts.Interfaces.IAccessPolicyQueryRepository;

namespace UserManagement.Infrastructure.Repositories.AccessPolicy
{
    // Implements the minimal Contracts interface used by AccessPolicyService in Shared.Infrastructure.
    // Kept separate from AccessPolicyQueryRepository (Application CRUD interface) to stay focused.
    internal sealed class AccessPolicyEvaluationRepository : IEvalRepo
    {
        private readonly IDbConnection _db;

        public AccessPolicyEvaluationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<bool> CheckBypassAsync(int userId)
        {
            const string sql = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM [AppSecurity].[UserRoleAllocation] ura
                    INNER JOIN [AppSecurity].[UserRole] ur ON ur.Id = ura.UserRoleId
                    WHERE ura.UserId      = @UserId
                      AND ura.IsActive    = 1
                      AND ur.BypassDataAccess = 1
                      AND ur.IsDeleted    = 0
                ) THEN 1 ELSE 0 END
                """;

            return await _db.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
        }

        public async Task<IReadOnlyList<int>> GetUserRoleIdsAsync(int userId)
        {
            const string sql = """
                SELECT ura.UserRoleId
                FROM [AppSecurity].[UserRoleAllocation] ura
                WHERE ura.UserId = @UserId AND ura.IsActive = 1
                """;

            var result = await _db.QueryAsync<int>(sql, new { UserId = userId });
            return result.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<int>?> GetAllowedValueIdsAsync(
            string policyCode, IEnumerable<int> roleIds)
        {
            var roleIdList = roleIds.ToList();

            // If policy code is not configured → return null (unrestricted)
            const string policyExistsSql = """
                SELECT COUNT(1)
                FROM [AppSecurity].[AccessPolicy]
                WHERE PolicyCode = @PolicyCode AND IsActive = 1 AND IsDeleted = 0
                """;

            var policyExists = await _db.ExecuteScalarAsync<int>(
                policyExistsSql, new { PolicyCode = policyCode });

            if (policyExists == 0)
                return null;

            if (roleIdList.Count == 0)
                return Array.Empty<int>();

            const string valuesSql = """
                SELECT DISTINCT rap.ValueId
                FROM [AppSecurity].[RoleAccessPolicy] rap
                INNER JOIN [AppSecurity].[AccessPolicy] ap ON ap.Id = rap.AccessPolicyId
                WHERE ap.PolicyCode = @PolicyCode
                  AND ap.IsActive   = 1
                  AND ap.IsDeleted  = 0
                  AND rap.RoleId IN @RoleIds
                """;

            var values = await _db.QueryAsync<int>(
                valuesSql, new { PolicyCode = policyCode, RoleIds = roleIdList });

            return values.ToList().AsReadOnly();
        }
    }
}
