using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.Infrastructure.Repositories.AccountGroup
{
    public class AccountGroupQueryRepository : IAccountGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public AccountGroupQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        // ── Flat row used to assemble the nested tree ──────────────────────────
        private sealed class AccountGroupRow
        {
            public int Id { get; set; }
            public int CompanyId { get; set; }
            public int? AccountTypeId { get; set; }
            public string? GroupCode { get; set; }
            public string? GroupName { get; set; }
            public int? ParentAccountGroupId { get; set; }
            public int Level { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; }
            public bool IsLeaf { get; set; }
        }

        public async Task<List<AccountGroupTreeDto>> GetTreeAsync(int? companyId)
        {
            const string sql = @"
                SELECT ag.Id, ag.CompanyId, ag.AccountTypeId, ag.GroupCode, ag.GroupName, ag.ParentAccountGroupId,
                       ag.Level, ag.SortOrder, ag.IsActive, ag.IsLeaf
                FROM [Finance].[AccountGroup] ag
                WHERE ag.IsDeleted = 0
                  AND (@CompanyId IS NULL OR ag.CompanyId = @CompanyId)
                ORDER BY ag.Level, ag.SortOrder, ag.GroupCode";

            var rows = (await _dbConnection.QueryAsync<AccountGroupRow>(sql, new { CompanyId = companyId })).ToList();

            var nodes = rows.ToDictionary(
                r => r.Id,
                r => new AccountGroupTreeDto
                {
                    Id = r.Id,
                    CompanyId = r.CompanyId,
                    AccountTypeId = r.AccountTypeId,
                    GroupCode = r.GroupCode,
                    GroupName = r.GroupName,
                    ParentAccountGroupId = r.ParentAccountGroupId,
                    Level = r.Level,
                    SortOrder = r.SortOrder,
                    IsActive = r.IsActive,
                    IsLeaf = r.IsLeaf
                });

            var roots = new List<AccountGroupTreeDto>();
            foreach (var row in rows)
            {
                var node = nodes[row.Id];
                if (row.ParentAccountGroupId.HasValue && nodes.TryGetValue(row.ParentAccountGroupId.Value, out var parent))
                {
                    parent.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            // ChildrenCount reflects the actual children; IsLeaf is the stored flag.
            foreach (var node in nodes.Values)
                node.ChildrenCount = node.Children.Count;

            return roots;
        }

        public async Task<AccountGroupDetailDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT ag.Id, ag.CompanyId, ag.AccountTypeId, at.AccountTypeName,
                       ag.GroupCode, ag.GroupName, ag.ParentAccountGroupId,
                       ag.Level, ag.SortOrder, ag.IsLeaf,
                       p.GroupName AS ParentGroupName,
                       (SELECT COUNT(1) FROM [Finance].[AccountGroup] c
                        WHERE c.ParentAccountGroupId = ag.Id AND c.IsDeleted = 0) AS ChildrenCount,
                       ag.IsActive, ag.IsDeleted,
                       ag.CreatedBy, ag.CreatedDate, ag.CreatedByName, ag.CreatedIP,
                       ag.ModifiedBy, ag.ModifiedDate, ag.ModifiedByName, ag.ModifiedIP
                FROM [Finance].[AccountGroup] ag
                LEFT JOIN [Finance].[AccountGroup] p ON ag.ParentAccountGroupId = p.Id AND p.IsDeleted = 0
                LEFT JOIN [Finance].[AccountTypeMaster] at ON ag.AccountTypeId = at.Id AND at.IsDeleted = 0
                WHERE ag.IsDeleted = 0 AND ag.Id = @Id";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AccountGroupDetailDto>(sql, new { Id = id });

            if (dto == null)
                return null;

            // Cross-module company name (UserManagement).
            var companies = await _companyLookup.GetAllCompanyAsync();
            dto.CompanyName = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId)?.CompanyName;

            dto.Path = await BuildPathAsync(id);

            return dto;
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var search = string.IsNullOrWhiteSpace(term) ? "%" : $"%{term}%";

            const string sql = @"
                SELECT TOP 20 ag.Id, ag.GroupCode, ag.GroupName, ag.Level
                FROM [Finance].[AccountGroup] ag
                WHERE ag.IsDeleted = 0 AND ag.IsActive = 1
                  AND (ag.GroupCode LIKE @Search OR ag.GroupName LIKE @Search)
                ORDER BY ag.Level, ag.GroupCode";

            var result = await _dbConnection.QueryAsync<AccountGroupLookupDto>(sql, new { Search = search });
            return result.ToList();
        }

        public async Task<IReadOnlyList<AccountGroupLookupDto>> GetParentsByLevelAsync(int level, int? companyId)
        {
            const string sql = @"
                SELECT ag.Id, ag.GroupCode, ag.GroupName, ag.Level
                FROM [Finance].[AccountGroup] ag
                WHERE ag.IsDeleted = 0 AND ag.IsActive = 1 AND ag.Level = @Level
                  AND (@CompanyId IS NULL OR ag.CompanyId = @CompanyId)
                ORDER BY ag.SortOrder, ag.GroupCode";

            var result = await _dbConnection.QueryAsync<AccountGroupLookupDto>(sql, new { Level = level, CompanyId = companyId });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string groupCode, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[AccountGroup]
                WHERE GroupCode = @GroupCode AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { GroupCode = groupCode, ExcludeId = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[AccountGroup]
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ParentExistsAsync(int parentId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[AccountGroup]
                WHERE Id = @ParentId AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ParentId = parentId });
            return count > 0;
        }

        public async Task<int?> GetLevelAsync(int id)
        {
            const string sql = @"
                SELECT TOP 1 Level FROM [Finance].[AccountGroup]
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { Id = id });
        }

        public async Task<bool> IsDescendantAsync(int ancestorId, int candidateId)
        {
            // Walks the subtree rooted at ancestorId; true if candidateId is inside it.
            const string sql = @"
                WITH Subtree AS (
                    SELECT Id FROM [Finance].[AccountGroup]
                    WHERE Id = @AncestorId AND IsDeleted = 0
                    UNION ALL
                    SELECT ag.Id FROM [Finance].[AccountGroup] ag
                    INNER JOIN Subtree s ON ag.ParentAccountGroupId = s.Id
                    WHERE ag.IsDeleted = 0
                )
                SELECT COUNT(1) FROM Subtree WHERE Id = @CandidateId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { AncestorId = ancestorId, CandidateId = candidateId });
            return count > 0;
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[AccountGroup]
                WHERE ParentAccountGroupId = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task<string> BuildPathAsync(int id)
        {
            const string sql = @"
                WITH Ancestors AS (
                    SELECT Id, GroupName, ParentAccountGroupId, Level
                    FROM [Finance].[AccountGroup]
                    WHERE Id = @Id AND IsDeleted = 0
                    UNION ALL
                    SELECT p.Id, p.GroupName, p.ParentAccountGroupId, p.Level
                    FROM [Finance].[AccountGroup] p
                    INNER JOIN Ancestors a ON p.Id = a.ParentAccountGroupId
                    WHERE p.IsDeleted = 0
                )
                SELECT GroupName FROM Ancestors ORDER BY Level";

            var names = (await _dbConnection.QueryAsync<string>(sql, new { Id = id })).ToList();
            return string.Join(" → ", names);
        }
    }
}
