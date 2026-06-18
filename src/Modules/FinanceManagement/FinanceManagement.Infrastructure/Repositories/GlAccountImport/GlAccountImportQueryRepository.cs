using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountImport
{
    public class GlAccountImportQueryRepository : IGlAccountImportQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public GlAccountImportQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        // ── Reference data (one shot — the perf key for AC4) ──────────────────
        public async Task<GlAccountImportReferenceData> LoadReferenceDataAsync(int companyId, CancellationToken ct)
        {
            var data = new GlAccountImportReferenceData { CompanyId = companyId };

            // Groups + each group's Level-1 ancestor account type.
            const string groupSql = @"
                WITH Roots AS (
                    SELECT Id, Id AS RootId, AccountTypeId
                    FROM Finance.AccountGroup
                    WHERE ParentAccountGroupId IS NULL AND IsDeleted = 0 AND CompanyId = @CompanyId
                    UNION ALL
                    SELECT c.Id, r.RootId, r.AccountTypeId
                    FROM Finance.AccountGroup c
                    JOIN Roots r ON c.ParentAccountGroupId = r.Id
                    WHERE c.IsDeleted = 0
                )
                SELECT g.Id, g.GroupCode, g.[Level], g.ParentAccountGroupId, g.IsLeaf,
                       r.AccountTypeId AS RootAccountTypeId
                FROM Finance.AccountGroup g
                LEFT JOIN Roots r ON g.Id = r.Id
                WHERE g.IsDeleted = 0 AND g.CompanyId = @CompanyId;";

            var groups = await _dbConnection.QueryAsync<GroupRefRow>(
                new CommandDefinition(groupSql, new { CompanyId = companyId }, cancellationToken: ct));
            foreach (var g in groups)
            {
                var key = Key(g.GroupCode);
                if (key == null || data.GroupsByCode.ContainsKey(key)) continue;
                data.GroupsByCode[key] = new ExistingGroupRef
                {
                    Id = g.Id,
                    GroupCode = g.GroupCode,
                    Level = g.Level,
                    ParentAccountGroupId = g.ParentAccountGroupId,
                    IsLeaf = g.IsLeaf,
                    RootAccountTypeId = g.RootAccountTypeId
                };
            }

            // Account types (statutory heads).
            const string typeSql = @"
                SELECT Id, AccountTypeName, StartCode, AccountCodeLength
                FROM Finance.AccountTypeMaster
                WHERE CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0;";
            var types = await _dbConnection.QueryAsync<AccountTypeFormatRef>(
                new CommandDefinition(typeSql, new { CompanyId = companyId }, cancellationToken: ct));
            foreach (var t in types)
            {
                data.AccountTypesById[t.Id] = t;
                var key = Key(t.AccountTypeName);
                if (key != null) data.AccountTypesByName[key] = t;
            }

            // Normal balance + sub-ledger type (MiscMaster by MiscType code).
            data.NormalBalanceByCode = await LoadMiscByCodeAsync("NB", ct);
            data.SubLedgerTypeByCode = await LoadMiscByCodeAsync("SLTYPE", ct);

            // Currency types (company-scoped).
            const string currencySql = @"
                SELECT CurrencyTypeCode AS Code, Id
                FROM Finance.CurrencyForexConfig
                WHERE CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0;";
            var currencies = await _dbConnection.QueryAsync<CodeRow>(
                new CommandDefinition(currencySql, new { CompanyId = companyId }, cancellationToken: ct));
            data.CurrencyByCode = ToCodeMap(currencies);

            // Existing account code/name sets for duplicate checks.
            const string acctSql = @"
                SELECT AccountCode, AccountName
                FROM Finance.GlAccountMaster
                WHERE CompanyId = @CompanyId AND IsDeleted = 0;";
            var accts = await _dbConnection.QueryAsync<AccountCodeNameRow>(
                new CommandDefinition(acctSql, new { CompanyId = companyId }, cancellationToken: ct));
            foreach (var a in accts)
            {
                var ck = Key(a.AccountCode);
                if (ck != null) data.ExistingAccountCodes.Add(ck);
                var nk = Key(a.AccountName);
                if (nk != null) data.ExistingAccountNames.Add(nk);
            }

            return data;
        }

        private async Task<Dictionary<string, int>> LoadMiscByCodeAsync(string miscTypeCode, CancellationToken ct)
        {
            const string sql = @"
                SELECT mm.Code AS Code, mm.Id AS Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.IsActive = 1 AND mm.IsDeleted = 0 AND mtm.MiscTypeCode = @MiscTypeCode;";
            var rows = await _dbConnection.QueryAsync<CodeRow>(
                new CommandDefinition(sql, new { MiscTypeCode = miscTypeCode }, cancellationToken: ct));
            return ToCodeMap(rows);
        }

        // ── Export rows (groups then accounts; code-based, round-trips — AC5) ──
        public async Task<IReadOnlyList<GlAccountImportRowDto>> GetExportRowsAsync(int companyId, CancellationToken ct)
        {
            var rows = new List<GlAccountImportRowDto>();

            const string groupSql = @"
                SELECT g.GroupCode,
                       g.GroupName,
                       p.GroupCode AS ParentGroupCode,
                       CASE WHEN g.ParentAccountGroupId IS NULL THEN atype.AccountTypeName ELSE NULL END AS AccountType,
                       g.SortOrder
                FROM Finance.AccountGroup g
                LEFT JOIN Finance.AccountGroup p ON g.ParentAccountGroupId = p.Id
                LEFT JOIN Finance.AccountTypeMaster atype ON g.AccountTypeId = atype.Id
                WHERE g.IsDeleted = 0 AND g.CompanyId = @CompanyId
                ORDER BY g.[Level] ASC, g.SortOrder ASC, g.GroupCode ASC;";

            var groups = await _dbConnection.QueryAsync<GroupExportRow>(
                new CommandDefinition(groupSql, new { CompanyId = companyId }, cancellationToken: ct));
            foreach (var g in groups)
            {
                rows.Add(new GlAccountImportRowDto
                {
                    RecordType = GlAccountImportColumns.RecordTypeGroup,
                    GroupCode = g.GroupCode,
                    GroupName = g.GroupName,
                    ParentGroupCode = g.ParentGroupCode,
                    AccountType = g.AccountType,
                    SortOrder = g.SortOrder.ToString()
                });
            }

            const string accountSql = @"
                SELECT am.AccountCode,
                       am.AccountName,
                       am.Description,
                       ag.GroupCode AS AccountGroupCode,
                       nb.Code AS NormalBalance,
                       cfc.CurrencyTypeCode AS Currency,
                       slt.Code AS SubLedgerType,
                       am.IsCostCentreMandatory,
                       am.IsTaxRelevant,
                       am.IsInterCompany,
                       am.IsReconciliationRequired
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountGroup ag ON am.AccountGroupId = ag.Id
                LEFT JOIN Finance.MiscMaster nb ON am.NormalBalanceId = nb.Id
                LEFT JOIN Finance.MiscMaster slt ON am.SubLedgerTypeId = slt.Id
                LEFT JOIN Finance.CurrencyForexConfig cfc ON am.CurrencyTypeId = cfc.Id
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId
                ORDER BY am.AccountCode ASC;";

            var accounts = await _dbConnection.QueryAsync<AccountExportRow>(
                new CommandDefinition(accountSql, new { CompanyId = companyId }, cancellationToken: ct));
            foreach (var a in accounts)
            {
                rows.Add(new GlAccountImportRowDto
                {
                    RecordType = GlAccountImportColumns.RecordTypeAccount,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Description = a.Description,
                    AccountGroupCode = a.AccountGroupCode,
                    NormalBalance = a.NormalBalance,
                    Currency = a.Currency,
                    SubLedgerType = a.SubLedgerType,
                    IsCostCentreMandatory = Bit(a.IsCostCentreMandatory),
                    IsTaxRelevant = Bit(a.IsTaxRelevant),
                    IsInterCompany = Bit(a.IsInterCompany),
                    IsReconciliationRequired = Bit(a.IsReconciliationRequired)
                });
            }

            return rows;
        }

        // ── Logs / errors ─────────────────────────────────────────────────────
        public async Task<(List<GlAccountImportLogDto> Logs, int TotalCount)> GetLogsAsync(int companyId, int pageNumber, int pageSize)
        {
            const string sql = @"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.GlAccountImportLog
                WHERE CompanyId = @CompanyId AND IsDeleted = 0;

                SELECT Id, CompanyId, FileName, FileFormat, ImportMode, Status,
                       TotalRows, GroupRows, AccountRows, ValidRows, InvalidRows,
                       ImportedGroups, ImportedAccounts, SkippedRows, DurationMs,
                       CreatedBy, CreatedByName, CreatedDate
                FROM Finance.GlAccountImportLog
                WHERE CompanyId = @CompanyId AND IsDeleted = 0
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                CompanyId = companyId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var logs = (await multi.ReadAsync<GlAccountImportLogDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (logs.Count > 0)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                foreach (var log in logs)
                    log.CompanyName = companyDict.TryGetValue(log.CompanyId, out var name) ? name : null;
            }

            return (logs, totalCount);
        }

        public async Task<IReadOnlyList<GlAccountImportErrorDto>> GetErrorsAsync(int importLogId)
        {
            const string sql = @"
                SELECT RowNumber, RecordType, ColumnName, AttemptedValue, ErrorMessage
                FROM Finance.GlAccountImportError
                WHERE ImportLogId = @ImportLogId AND IsDeleted = 0
                ORDER BY RowNumber ASC, Id ASC;";

            var errors = await _dbConnection.QueryAsync<GlAccountImportErrorDto>(sql, new { ImportLogId = importLogId });
            return errors.ToList();
        }

        public async Task<bool> LogBelongsToCompanyAsync(int importLogId, int companyId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.GlAccountImportLog
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsDeleted = 0;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = importLogId, CompanyId = companyId });
            return count > 0;
        }

        // ── helpers ───────────────────────────────────────────────────────────
        private static Dictionary<string, int> ToCodeMap(IEnumerable<CodeRow> rows)
        {
            var map = new Dictionary<string, int>();
            foreach (var r in rows)
            {
                var key = Key(r.Code);
                if (key != null && !map.ContainsKey(key)) map[key] = r.Id;
            }
            return map;
        }

        private static string? Key(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

        private static string Bit(bool value) => value ? "1" : "0";

        private sealed class GroupRefRow
        {
            public int Id { get; set; }
            public string GroupCode { get; set; } = null!;
            public int Level { get; set; }
            public int? ParentAccountGroupId { get; set; }
            public bool IsLeaf { get; set; }
            public int? RootAccountTypeId { get; set; }
        }

        private sealed class CodeRow
        {
            public string? Code { get; set; }
            public int Id { get; set; }
        }

        private sealed class AccountCodeNameRow
        {
            public string? AccountCode { get; set; }
            public string? AccountName { get; set; }
        }

        private sealed class GroupExportRow
        {
            public string GroupCode { get; set; } = null!;
            public string GroupName { get; set; } = null!;
            public string? ParentGroupCode { get; set; }
            public string? AccountType { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class AccountExportRow
        {
            public string AccountCode { get; set; } = null!;
            public string AccountName { get; set; } = null!;
            public string? Description { get; set; }
            public string? AccountGroupCode { get; set; }
            public string? NormalBalance { get; set; }
            public string? Currency { get; set; }
            public string? SubLedgerType { get; set; }
            public bool IsCostCentreMandatory { get; set; }
            public bool IsTaxRelevant { get; set; }
            public bool IsInterCompany { get; set; }
            public bool IsReconciliationRequired { get; set; }
        }
    }
}
