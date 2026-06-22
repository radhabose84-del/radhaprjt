using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountMaster
{
    public class GlAccountMasterQueryRepository : IGlAccountMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public GlAccountMasterQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        private const string BaseSelect = @"
            am.Id, am.CompanyId,
            am.AccountTypeId, atype.AccountTypeName, atype.StartCode, atype.AccountCodeLength,
            am.AccountGroupId, ag.GroupCode AS AccountGroupCode, ag.GroupName AS AccountGroupName,
            am.AccountCode, am.AccountName, am.Description,
            am.NormalBalanceId, nb.Code AS NormalBalanceCode, nb.Description AS NormalBalanceName,
            am.CurrencyTypeId, cfc.CurrencyTypeCode, cfc.CurrencyTypeName,
            am.SubLedgerTypeId, slt.Code AS SubLedgerTypeCode, slt.Description AS SubLedgerTypeName,
            am.IsCostCentreMandatory, am.IsProfitCentreMandatory, am.IsTaxRelevant, am.IsInterCompany, am.IsReconciliationRequired,
            tal.Id AS TaxAccountLinkageId, tal.TaxCodeId, tc.TaxCode, tc.TaxName,
            -- Control account type = the GL account's SubLedgerType (SLTYPE) when it isn't 'NONE'.
            CASE WHEN slt.Code <> 'NONE' THEN am.SubLedgerTypeId END AS ControlAccountTypeId,
            CASE WHEN slt.Code <> 'NONE' THEN slt.Description END AS ControlAccountType,
            -- US-GL02-08B (AC3 / G3): mark accounts touched by a committed post-freeze change.
            am.LastPostFreezeChangeOn,
            CASE WHEN am.LastPostFreezeChangeOn IS NOT NULL THEN 1 ELSE 0 END AS IsPostFreeze,
            am.IsActive, am.IsDeleted,
            am.CreatedBy, am.CreatedDate, am.CreatedByName, am.CreatedIP,
            am.ModifiedBy, am.ModifiedDate, am.ModifiedByName, am.ModifiedIP
        ";

        private const string BaseFromAndJoins = @"
            FROM Finance.GlAccountMaster am
            LEFT JOIN Finance.AccountTypeMaster atype ON am.AccountTypeId = atype.Id AND atype.IsDeleted = 0
            LEFT JOIN Finance.AccountGroup       ag    ON am.AccountGroupId = ag.Id AND ag.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster         nb    ON am.NormalBalanceId = nb.Id AND nb.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster         slt   ON am.SubLedgerTypeId = slt.Id AND slt.IsDeleted = 0
            LEFT JOIN Finance.CurrencyForexConfig cfc  ON am.CurrencyTypeId = cfc.Id AND cfc.IsDeleted = 0
            LEFT JOIN Finance.TaxAccountLinkage   tal   ON tal.GlAccountId = am.Id AND tal.IsActive = 1
                AND CAST(GETDATE() AS date) >= tal.EffectiveFrom
                AND (tal.EffectiveTo IS NULL OR CAST(GETDATE() AS date) <= tal.EffectiveTo)
            LEFT JOIN Finance.TaxCodeMaster       tc    ON tal.TaxCodeId = tc.Id
        ";

        public async Task<(List<GlAccountMasterDto>, int)> GetAllAsync(int? pageNumber, int? pageSize, string? searchTerm, int companyId, int? accountTypeId = null, int? accountGroupId = null)
        {
            var whereClause = "am.IsDeleted = 0 AND am.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (am.AccountCode LIKE @Search OR am.AccountName LIKE @Search)";
            if (accountTypeId.HasValue && accountTypeId.Value > 0)
                whereClause += " AND am.AccountTypeId = @AccountTypeId";
            if (accountGroupId.HasValue && accountGroupId.Value > 0)
                whereClause += " AND am.AccountGroupId = @AccountGroupId";

            // No (or non-positive) page size → return every matching row (no OFFSET/FETCH paging).
            var isPaged = pageNumber.HasValue && pageNumber.Value > 0
                          && pageSize.HasValue && pageSize.Value > 0;

            var pagingClause = isPaged
                ? "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"
                : string.Empty;

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.GlAccountMaster am
                WHERE {whereClause};

                SELECT {BaseSelect}
                {BaseFromAndJoins}
                WHERE {whereClause}
                ORDER BY am.AccountCode ASC, am.Id DESC
                {pagingClause};

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                AccountTypeId = accountTypeId,
                AccountGroupId = accountGroupId,
                Offset = isPaged ? (pageNumber!.Value - 1) * pageSize!.Value : 0,
                PageSize = isPaged ? pageSize!.Value : 0
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GlAccountMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                foreach (var item in list)
                {
                    item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var name) ? name : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<GlAccountMasterDto?> GetByIdAsync(int id)
        {
            var sql = $@"
                SELECT {BaseSelect}
                {BaseFromAndJoins}
                WHERE am.Id = @Id AND am.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<GlAccountMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var company = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId);
                dto.CompanyName = company?.CompanyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<GlAccountMasterLookupDto>> AutocompleteAsync(string term, int companyId, string? accountTypeCode, CancellationToken ct)
        {
            var whereClause = "am.IsDeleted = 0 AND am.IsActive = 1 AND am.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (am.AccountCode LIKE @Term OR am.AccountName LIKE @Term OR l3.L3Name LIKE @Term)";

            string joinTypeClause = string.Empty;
            if (!string.IsNullOrWhiteSpace(accountTypeCode))
            {
                joinTypeClause = " AND atype.AccountTypeName = @AccountTypeCode";
            }

            // L3Map: every Level-3 node is its own L3 ancestor; its descendants (L4/L5/L6) inherit it.
            var sql = $@"
                WITH L3Map AS (
                    SELECT Id, Id AS L3Id, GroupCode AS L3Code, GroupName AS L3Name
                    FROM Finance.AccountGroup WHERE [Level] = 3 AND IsDeleted = 0
                    UNION ALL
                    SELECT c.Id, m.L3Id, m.L3Code, m.L3Name
                    FROM Finance.AccountGroup c
                    JOIN L3Map m ON c.ParentAccountGroupId = m.Id
                    WHERE c.IsDeleted = 0
                )
                SELECT am.Id, am.CompanyId, am.AccountTypeId,
                       am.AccountCode, am.AccountName,
                       nb.Code AS NormalBalanceCode,
                       l3.L3Id AS L3AccountGroupId, l3.L3Code AS L3GroupCode, l3.L3Name AS L3GroupName
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountTypeMaster atype ON am.AccountTypeId = atype.Id AND atype.IsDeleted = 0{joinTypeClause}
                LEFT JOIN Finance.MiscMaster nb ON am.NormalBalanceId = nb.Id AND nb.IsDeleted = 0
                LEFT JOIN L3Map l3 ON am.AccountGroupId = l3.Id
                WHERE {whereClause}
                ORDER BY am.AccountCode ASC";

            var result = await _dbConnection.QueryAsync<GlAccountMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId, AccountTypeCode = accountTypeCode }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string accountCode, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.GlAccountMaster
                WHERE AccountCode = @AccountCode AND CompanyId = @CompanyId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { AccountCode = accountCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> AlreadyExistsByNameAsync(string accountName, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.GlAccountMaster
                WHERE AccountName = @AccountName AND CompanyId = @CompanyId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { AccountName = accountName.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.GlAccountMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> AccountTypeExistsForCompanyAsync(int accountTypeId, int companyId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountTypeMaster
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = accountTypeId, CompanyId = companyId });
            return count > 0;
        }

        public async Task<bool> AccountGroupExistsForCompanyAsync(int accountGroupId, int companyId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountGroup
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = accountGroupId, CompanyId = companyId });
            return count > 0;
        }

        public async Task<bool> AccountGroupIsLeafForCompanyAsync(int accountGroupId, int companyId)
        {
            // Accounts attach only at a leaf group (no children) of the same company.
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountGroup
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsLeaf = 1 AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = accountGroupId, CompanyId = companyId });
            return count > 0;
        }

        public async Task<bool> NormalBalanceExistsAsync(int normalBalanceId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'NB'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = normalBalanceId });
            return count > 0;
        }

        public async Task<bool> SubLedgerTypeExistsAsync(int subLedgerTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'SLTYPE'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = subLedgerTypeId });
            return count > 0;
        }

        public async Task<bool> CurrencyTypeExistsForCompanyAsync(int currencyTypeId, int companyId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.CurrencyForexConfig
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = currencyTypeId, CompanyId = companyId });
            return count > 0;
        }

        public async Task<(int AccountCodeLength, string? StartCode, string? AccountTypeName)?> GetAccountTypeFormatAsync(int accountTypeId)
        {
            const string sql = @"
                SELECT AccountCodeLength, StartCode, AccountTypeName
                FROM Finance.AccountTypeMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<AccountTypeFormatRow>(sql, new { Id = accountTypeId });
            if (row == null) return null;
            return (row.AccountCodeLength, row.StartCode, row.AccountTypeName);
        }

        // Posting-guard stubs reserved for later (JournalEntry / open period not yet built).
        public Task<bool> SoftDeleteValidationAsync(int id) => Task.FromResult(false);

        public Task<bool> IsGlAccountLinkedAsync(int id) => Task.FromResult(false);

        // ── US-GL02-07 type-ahead ──────────────────────────────────────────────
        private const string SearchSelect = @"
            am.Id, am.AccountCode, am.AccountName,
            am.AccountTypeId, atype.AccountTypeName,
            am.AccountGroupId, ag.GroupName AS AccountGroupName,
            am.IsActive";

        public async Task<IReadOnlyList<AccountSearchResultDto>> SearchAsync(
            string? term, int companyId, int? accountTypeId, int? accountGroupId, bool activeOnly, int take, CancellationToken ct)
        {
            var hasTerm = !string.IsNullOrWhiteSpace(term);
            var topN = take > 0 ? take : 20;

            // GroupSubtree expands the chosen group to all descendant groups (accounts attach at leaves),
            // so the group filter matches the whole branch. Empty when no group filter is supplied.
            var sql = $@"
                WITH GroupSubtree AS (
                    SELECT Id FROM Finance.AccountGroup
                    WHERE @AccountGroupId IS NOT NULL AND Id = @AccountGroupId AND IsDeleted = 0
                    UNION ALL
                    SELECT c.Id FROM Finance.AccountGroup c
                    INNER JOIN GroupSubtree s ON c.ParentAccountGroupId = s.Id
                    WHERE c.IsDeleted = 0
                )
                SELECT TOP (@TopN) {SearchSelect}
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountTypeMaster atype ON am.AccountTypeId = atype.Id AND atype.IsDeleted = 0
                LEFT JOIN Finance.AccountGroup       ag    ON am.AccountGroupId = ag.Id AND ag.IsDeleted = 0
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId
                  AND (@ActiveOnly = 0 OR am.IsActive = 1)
                  AND (@AccountTypeId IS NULL OR am.AccountTypeId = @AccountTypeId)
                  AND (@AccountGroupId IS NULL OR am.AccountGroupId IN (SELECT Id FROM GroupSubtree))
                  AND (
                        @HasTerm = 0
                     OR am.AccountCode LIKE @Prefix
                     OR am.AccountName LIKE @Contains
                     OR am.Description LIKE @Contains
                     OR ag.GroupName LIKE @Contains
                     OR atype.AccountTypeName LIKE @Contains
                      )
                ORDER BY
                    CASE WHEN am.AccountCode LIKE @Prefix THEN 0 ELSE 1 END,   -- exact code-prefix first
                    am.AccountCode ASC, am.AccountName ASC
                OPTION (MAXRECURSION 32);";

            var parameters = new
            {
                CompanyId = companyId,
                AccountTypeId = accountTypeId,
                AccountGroupId = accountGroupId,
                ActiveOnly = activeOnly ? 1 : 0,
                HasTerm = hasTerm ? 1 : 0,
                Prefix = hasTerm ? $"{term!.Trim()}%" : null,
                Contains = hasTerm ? $"%{term!.Trim()}%" : null,
                TopN = topN
            };

            var rows = await _dbConnection.QueryAsync<AccountSearchResultDto>(
                new CommandDefinition(sql, parameters, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<IReadOnlyList<AccountSearchResultDto>> GetByIdsForSearchAsync(
            IReadOnlyCollection<int> ids, int companyId, CancellationToken ct)
        {
            if (ids == null || ids.Count == 0)
                return Array.Empty<AccountSearchResultDto>();

            var sql = $@"
                SELECT {SearchSelect}
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountTypeMaster atype ON am.AccountTypeId = atype.Id AND atype.IsDeleted = 0
                LEFT JOIN Finance.AccountGroup       ag    ON am.AccountGroupId = ag.Id AND ag.IsDeleted = 0
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId AND am.Id IN @Ids";

            var rows = await _dbConnection.QueryAsync<AccountSearchResultDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, Ids = ids }, cancellationToken: ct));
            return rows.ToList();
        }

        private sealed class AccountTypeFormatRow
        {
            public int AccountCodeLength { get; set; }
            public string? StartCode { get; set; }
            public string? AccountTypeName { get; set; }
        }
    }
}
