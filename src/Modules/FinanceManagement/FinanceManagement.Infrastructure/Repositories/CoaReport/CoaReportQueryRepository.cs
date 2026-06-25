using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;

namespace FinanceManagement.Infrastructure.Repositories.CoaReport
{
    // US-GL02-15 COA Listing & Structure Reports. Read-only set-based aggregates. Posting counts are
    // pre-aggregated in derived tables (keyed on JournalDetail.GlAccountId) so a 2,000-account report
    // stays well inside the AC2 budget; posted = MiscMaster JOURNAL_STATUS/POSTED.
    public class CoaReportQueryRepository : ICoaReportQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CoaReportQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Derived posting aggregate (posted vouchers only) — reused by listing + usage.
        private const string PostedAggregate = @"
            SELECT jd.GlAccountId, COUNT(*) AS PostingCount, MAX(jh.PostingDate) AS LastPostingDate
            FROM Finance.JournalDetail jd
            INNER JOIN Finance.JournalHeader jh ON jh.Id = jd.JournalHeaderId AND jh.IsDeleted = 0
            INNER JOIN Finance.MiscMaster st ON st.Id = jh.StatusId
            INNER JOIN Finance.MiscTypeMaster stype ON stype.Id = st.MiscTypeId
            WHERE jh.CompanyId = @CompanyId AND stype.MiscTypeCode = 'JOURNAL_STATUS' AND st.Code = 'POSTED'
            GROUP BY jd.GlAccountId";

        private const string BalanceAggregate = @"
            SELECT GlAccountId, SUM(DrTotal - CrTotal) AS Balance
            FROM Finance.LedgerBalance
            WHERE CompanyId = @CompanyId
            GROUP BY GlAccountId";

        public async Task<List<CoaListingItemDto>> GetCoaListingAsync(
            int companyId, int? accountTypeId, int? accountGroupId, bool activeOnly, string? searchTerm, CancellationToken ct)
        {
            var p = new DynamicParameters();
            p.Add("CompanyId", companyId);
            p.Add("AccountTypeId", accountTypeId);
            p.Add("AccountGroupId", accountGroupId);
            p.Add("ActiveOnly", activeOnly ? 1 : 0);
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            p.Add("HasSearch", hasSearch ? 1 : 0);
            p.Add("Search", hasSearch ? $"%{searchTerm!.Trim()}%" : null);

            var sql = $@"
                SELECT
                    am.Id, am.AccountCode, am.AccountName,
                    am.AccountTypeId, atype.AccountTypeName,
                    am.AccountGroupId, ag.GroupCode, ag.GroupName, ag.[Level] AS GroupLevel,
                    pag.GroupCode AS ParentGroupCode, pag.GroupName AS ParentGroupName,
                    nb.Code AS NormalBalanceCode, slt.Code AS SubLedgerTypeCode,
                    am.IsCostCentreMandatory, am.IsProfitCentreMandatory, am.IsTaxRelevant,
                    am.IsInterCompany, am.IsReconciliationRequired, am.IsActive,
                    ag.ScheduleIIISectionItemId, sii.LineCode AS FsLineCode, sii.LineName AS FsLineName,
                    stt.Code AS StatementTypeCode,
                    ISNULL(pc.PostingCount, 0) AS PostingCount, pc.LastPostingDate,
                    ISNULL(bal.Balance, 0) AS Balance
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountTypeMaster atype ON atype.Id = am.AccountTypeId AND atype.IsDeleted = 0
                LEFT JOIN Finance.AccountGroup ag ON ag.Id = am.AccountGroupId AND ag.IsDeleted = 0
                LEFT JOIN Finance.AccountGroup pag ON pag.Id = ag.ParentAccountGroupId AND pag.IsDeleted = 0
                LEFT JOIN Finance.MiscMaster nb ON nb.Id = am.NormalBalanceId AND nb.IsDeleted = 0
                LEFT JOIN Finance.MiscMaster slt ON slt.Id = am.SubLedgerTypeId AND slt.IsDeleted = 0
                LEFT JOIN Finance.ScheduleIIISectionItem sii ON sii.Id = ag.ScheduleIIISectionItemId AND sii.IsDeleted = 0
                LEFT JOIN Finance.ScheduleIIISection sec ON sec.Id = sii.SectionId AND sec.IsDeleted = 0
                LEFT JOIN Finance.MiscMaster stt ON stt.Id = sec.StatementTypeId AND stt.IsDeleted = 0
                LEFT JOIN ({PostedAggregate}) pc ON pc.GlAccountId = am.Id
                LEFT JOIN ({BalanceAggregate}) bal ON bal.GlAccountId = am.Id
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId
                  AND (@ActiveOnly = 0 OR am.IsActive = 1)
                  AND (@AccountTypeId IS NULL OR am.AccountTypeId = @AccountTypeId)
                  AND (@AccountGroupId IS NULL OR am.AccountGroupId = @AccountGroupId)
                  AND (@HasSearch = 0 OR am.AccountCode LIKE @Search OR am.AccountName LIKE @Search)
                ORDER BY am.AccountCode ASC, am.Id ASC";

            var rows = await _dbConnection.QueryAsync<CoaListingItemDto>(new CommandDefinition(sql, p, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<List<AccountUsageItemDto>> GetAccountUsageAsync(int companyId, int monthsSincePosting, CancellationToken ct)
        {
            var p = new DynamicParameters();
            p.Add("CompanyId", companyId);
            p.Add("Months", monthsSincePosting);

            // Active accounts that are never posted OR last posted before the cutoff.
            var sql = $@"
                SELECT
                    am.Id, am.AccountCode, am.AccountName, atype.AccountTypeName, ag.GroupName, am.IsActive,
                    ISNULL(pc.PostingCount, 0) AS PostingCount,
                    CAST(CASE WHEN pc.GlAccountId IS NULL THEN 1 ELSE 0 END AS bit) AS NeverPosted,
                    pc.LastPostingDate,
                    CASE WHEN pc.LastPostingDate IS NULL THEN NULL
                         ELSE DATEDIFF(MONTH, pc.LastPostingDate, CAST(GETDATE() AS date)) END AS MonthsSincePosting,
                    stt.Code AS StatementTypeCode,
                    CAST(CASE WHEN stt.Code = 'BS' THEN 1 ELSE 0 END AS bit) AS IsBalanceSheet,
                    ISNULL(bal.Balance, 0) AS Balance
                FROM Finance.GlAccountMaster am
                LEFT JOIN Finance.AccountTypeMaster atype ON atype.Id = am.AccountTypeId AND atype.IsDeleted = 0
                LEFT JOIN Finance.AccountGroup ag ON ag.Id = am.AccountGroupId AND ag.IsDeleted = 0
                LEFT JOIN Finance.ScheduleIIISectionItem sii ON sii.Id = ag.ScheduleIIISectionItemId AND sii.IsDeleted = 0
                LEFT JOIN Finance.ScheduleIIISection sec ON sec.Id = sii.SectionId AND sec.IsDeleted = 0
                LEFT JOIN Finance.MiscMaster stt ON stt.Id = sec.StatementTypeId AND stt.IsDeleted = 0
                LEFT JOIN ({PostedAggregate}) pc ON pc.GlAccountId = am.Id
                LEFT JOIN ({BalanceAggregate}) bal ON bal.GlAccountId = am.Id
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId AND am.IsActive = 1
                  AND (pc.LastPostingDate IS NULL
                       OR pc.LastPostingDate < DATEADD(MONTH, -@Months, CAST(GETDATE() AS date)))
                ORDER BY am.AccountCode ASC";

            var rows = (await _dbConnection.QueryAsync<AccountUsageItemDto>(new CommandDefinition(sql, p, cancellationToken: ct))).ToList();

            // AC3 — a balance-sheet account that still carries a balance is NOT a deactivation candidate.
            foreach (var row in rows)
            {
                if (row.IsBalanceSheet && row.Balance != 0m)
                {
                    row.IsDeactivationCandidate = false;
                    row.ExclusionReason = "Balance-sheet account with a non-zero balance";
                }
                else
                {
                    row.IsDeactivationCandidate = true;
                }
            }

            return rows;
        }

        public async Task<FsMappingValidationDto> GetFsMappingValidationAsync(int companyId, CancellationToken ct)
        {
            var p = new DynamicParameters();
            p.Add("CompanyId", companyId);

            const string sql = @"
                SELECT ag.Id AS AccountGroupId, ag.GroupCode, ag.GroupName, ag.[Level],
                       (SELECT COUNT(*) FROM Finance.GlAccountMaster gam
                        WHERE gam.AccountGroupId = ag.Id AND gam.IsDeleted = 0) AS AccountCount
                FROM Finance.AccountGroup ag
                WHERE ag.IsDeleted = 0 AND ag.CompanyId = @CompanyId AND ag.IsActive = 1
                  AND ag.IsLeaf = 1 AND ag.ScheduleIIISectionItemId IS NULL
                ORDER BY ag.[Level] ASC, ag.GroupCode ASC;

                SELECT
                    SUM(CASE WHEN ag.IsLeaf = 1 THEN 1 ELSE 0 END) AS TotalLeaf,
                    SUM(CASE WHEN ag.IsLeaf = 1 AND ag.ScheduleIIISectionItemId IS NOT NULL THEN 1 ELSE 0 END) AS Mapped
                FROM Finance.AccountGroup ag
                WHERE ag.IsDeleted = 0 AND ag.CompanyId = @CompanyId AND ag.IsActive = 1;";

            var multi = await _dbConnection.QueryMultipleAsync(new CommandDefinition(sql, p, cancellationToken: ct));
            var unmapped = (await multi.ReadAsync<FsMappingUnmappedItemDto>()).ToList();
            var counts = await multi.ReadFirstAsync<CountsRow>();

            return new FsMappingValidationDto
            {
                TotalLeafGroups = counts.TotalLeaf,
                MappedCount = counts.Mapped,
                UnmappedCount = unmapped.Count,
                Unmapped = unmapped
            };
        }

        private sealed class CountsRow
        {
            public int TotalLeaf { get; set; }
            public int Mapped { get; set; }
        }
    }
}
