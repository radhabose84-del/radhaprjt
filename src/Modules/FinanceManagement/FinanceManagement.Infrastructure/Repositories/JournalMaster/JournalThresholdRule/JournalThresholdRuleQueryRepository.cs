using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule
{
    public class JournalThresholdRuleQueryRepository : IJournalThresholdRuleQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public JournalThresholdRuleQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<JournalThresholdRuleDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "r.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND mm.Description LIKE @Search";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.JournalThresholdRule r
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = r.RuleTypeId
                WHERE {whereClause};

                SELECT r.Id, r.RuleTypeId, mm.Description AS RuleTypeName, r.ThresholdValue, r.Active, r.EffectiveFrom,
                    r.IsActive, r.IsDeleted,
                    r.CreatedBy, r.CreatedDate, r.CreatedByName, r.CreatedIP,
                    r.ModifiedBy, r.ModifiedDate, r.ModifiedByName, r.ModifiedIP
                FROM Finance.JournalThresholdRule r
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = r.RuleTypeId
                WHERE {whereClause}
                ORDER BY r.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<JournalThresholdRuleDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<JournalThresholdRuleDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT r.Id, r.RuleTypeId, mm.Description AS RuleTypeName, r.ThresholdValue, r.Active, r.EffectiveFrom,
                    r.IsActive, r.IsDeleted,
                    r.CreatedBy, r.CreatedDate, r.CreatedByName, r.CreatedIP,
                    r.ModifiedBy, r.ModifiedDate, r.ModifiedByName, r.ModifiedIP
                FROM Finance.JournalThresholdRule r
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = r.RuleTypeId
                WHERE r.Id = @Id AND r.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<JournalThresholdRuleDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<JournalThresholdRuleLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var whereClause = "r.IsDeleted = 0 AND r.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND mm.Description LIKE @Term";

            var sql = $@"
                SELECT r.Id, r.RuleTypeId, mm.Description AS RuleTypeName
                FROM Finance.JournalThresholdRule r
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = r.RuleTypeId
                WHERE {whereClause}
                ORDER BY mm.Description ASC";

            var result = await _dbConnection.QueryAsync<JournalThresholdRuleLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM Finance.JournalThresholdRule WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> RuleTypeExistsAsync(int ruleTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.MiscMaster mm
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                    WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0 AND mt.MiscTypeCode = 'THRESHOLD_RULE_TYPE'
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = ruleTypeId });
        }

        public async Task<(List<JournalFlagDto>, int)> GetFlagsAsync(int pageNumber, int pageSize, int? journalHeaderId)
        {
            var whereClause = "1 = 1";
            if (journalHeaderId.HasValue && journalHeaderId.Value > 0)
                whereClause += " AND f.JournalHeaderId = @JournalHeaderId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.JournalFlag f
                WHERE {whereClause};

                SELECT f.Id, f.JournalHeaderId, h.VoucherNo, f.RuleTypeId, mm.Description AS RuleTypeName,
                    f.Value, f.FlaggedAt, f.DigestSent
                FROM Finance.JournalFlag f
                LEFT JOIN Finance.JournalHeader h ON h.Id = f.JournalHeaderId
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = f.RuleTypeId
                WHERE {whereClause}
                ORDER BY f.FlaggedAt DESC, f.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                JournalHeaderId = journalHeaderId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<JournalFlagDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }
    }
}
