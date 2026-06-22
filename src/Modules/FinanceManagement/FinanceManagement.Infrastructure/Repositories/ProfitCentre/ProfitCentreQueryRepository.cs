using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Dto;

namespace FinanceManagement.Infrastructure.Repositories.ProfitCentre
{
    public class ProfitCentreQueryRepository : IProfitCentreQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUserLookup _userLookup;

        public ProfitCentreQueryRepository(
            IDbConnection dbConnection,
            ICompanyLookup companyLookup,
            IUserLookup userLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
            _userLookup = userLookup;
        }

        // Same-module joins: MiscMaster (level name) + self-join (parent name). Cross-module names
        // (Company/ResponsibleHead) are enriched via lookups below.
        private const string BaseSelect = @"
            pc.Id, pc.CompanyId,
            pc.ProfitCentreCode, pc.ProfitCentreName,
            pc.LevelId, ml.Description AS LevelName,
            pc.ParentProfitCentreId, parent.ProfitCentreName AS ParentProfitCentreName,
            pc.ResponsibleHeadId, pc.IsRevenueLinked, pc.MidYearJustification,
            pc.IsActive, pc.IsDeleted,
            pc.CreatedBy, pc.CreatedDate, pc.CreatedByName, pc.CreatedIP,
            pc.ModifiedBy, pc.ModifiedDate, pc.ModifiedByName, pc.ModifiedIP
        ";

        public async Task<(List<ProfitCentreDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "pc.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (pc.ProfitCentreCode LIKE @Search OR pc.ProfitCentreName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.ProfitCentre pc
                WHERE {whereClause};

                SELECT {BaseSelect}
                FROM Finance.ProfitCentre pc
                LEFT JOIN Finance.MiscMaster ml ON pc.LevelId = ml.Id
                LEFT JOIN Finance.ProfitCentre parent ON pc.ParentProfitCentreId = parent.Id
                WHERE {whereClause}
                ORDER BY pc.LevelId ASC, pc.ProfitCentreCode ASC, pc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ProfitCentreDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            await EnrichLookupNamesAsync(list);
            return (list, totalCount);
        }

        public async Task<ProfitCentreDto?> GetByIdAsync(int id)
        {
            var sql = $@"
                SELECT {BaseSelect}
                FROM Finance.ProfitCentre pc
                LEFT JOIN Finance.MiscMaster ml ON pc.LevelId = ml.Id
                LEFT JOIN Finance.ProfitCentre parent ON pc.ParentProfitCentreId = parent.Id
                WHERE pc.Id = @Id AND pc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<ProfitCentreDto>(sql, new { Id = id });

            if (dto != null)
                await EnrichLookupNamesAsync(new List<ProfitCentreDto> { dto });

            return dto;
        }

        public async Task<IReadOnlyList<ProfitCentreLookupDto>> AutocompleteAsync(string term, int? levelId, CancellationToken ct)
        {
            var whereClause = "pc.IsDeleted = 0 AND pc.IsActive = 1";
            if (levelId.HasValue && levelId.Value > 0)
                whereClause += " AND pc.LevelId = @LevelId";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (pc.ProfitCentreCode LIKE @Term OR pc.ProfitCentreName LIKE @Term)";

            var sql = $@"
                SELECT pc.Id, pc.ProfitCentreCode, pc.ProfitCentreName, pc.LevelId
                FROM Finance.ProfitCentre pc
                WHERE {whereClause}
                ORDER BY pc.ProfitCentreCode ASC";

            var result = await _dbConnection.QueryAsync<ProfitCentreLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", LevelId = levelId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string profitCentreCode, int? id = null)
        {
            // Uniqueness is GLOBAL — across all companies (AC#2). No company/unit filter.
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.ProfitCentre
                WHERE ProfitCentreCode = @Code AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = profitCentreCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.ProfitCentre
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<int> GetLevelSortOrderAsync(int levelId)
        {
            // The level row must belong to the PROFITCENTRELEVEL misc type; SortOrder is the stable ordinal (1/2).
            const string sql = @"
                SELECT mm.SortOrder
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mm.Id = @Id AND mm.IsDeleted = 0 AND mm.IsActive = 1
                  AND mt.MiscTypeCode = 'PROFITCENTRELEVEL'";

            var sort = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = levelId });
            return sort ?? 0;
        }

        public async Task<bool> ParentValidForLevelAsync(int? parentProfitCentreId, int levelId)
        {
            var childSort = await GetLevelSortOrderAsync(levelId);
            if (childSort <= 0)
                return false;                                   // invalid level — handled by its own rule

            if (childSort == 1)
                return !parentProfitCentreId.HasValue || parentProfitCentreId.Value == 0;  // L1 must have no parent

            if (!parentProfitCentreId.HasValue || parentProfitCentreId.Value <= 0)
                return false;                                   // L2 requires a parent

            // Parent must exist and sit exactly one level above the child (an L1 Segment for an L2).
            const string sql = @"
                SELECT ml.SortOrder
                FROM Finance.ProfitCentre pc
                INNER JOIN Finance.MiscMaster ml ON pc.LevelId = ml.Id
                WHERE pc.Id = @ParentId AND pc.IsDeleted = 0";

            var parentSort = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { ParentId = parentProfitCentreId.Value });
            return parentSort.HasValue && parentSort.Value == childSort - 1;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Block deleting a node that still has children. Current-year transactions are added when the
            // journal engine lands (HasCurrentYearTransactionsAsync stub below).
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.ProfitCentre
                    WHERE ParentProfitCentreId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            var hasChildren = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
            if (hasChildren)
                return true;

            return await HasCurrentYearTransactionsAsync(id);
        }

        // STUB — returns false until the journal engine tags transactions to profit centres (AC#5).
        // The behaviour and message are already wired; only this body changes when real data exists.
        public Task<bool> HasCurrentYearTransactionsAsync(int profitCentreId) => Task.FromResult(false);

        private async Task EnrichLookupNamesAsync(List<ProfitCentreDto> list)
        {
            if (list.Count == 0)
                return;

            // Company — group-level master spans companies; resolve names from the cached company lookup.
            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
            foreach (var item in list)
                item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var cName) ? cName : null;

            // Responsible Heads (optional — null until the FE wires the picker, but enrich if present).
            var headIds = list.Where(x => x.ResponsibleHeadId.HasValue).Select(x => x.ResponsibleHeadId!.Value).Distinct().ToList();
            if (headIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(headIds);
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                foreach (var item in list.Where(x => x.ResponsibleHeadId.HasValue))
                    item.ResponsibleHeadName = userDict.TryGetValue(item.ResponsibleHeadId!.Value, out var name) ? name : null;
            }
        }
    }
}
