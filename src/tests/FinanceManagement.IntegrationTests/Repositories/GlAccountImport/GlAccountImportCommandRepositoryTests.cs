using Dapper;
using Microsoft.Data.SqlClient;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;
using FinanceManagement.Infrastructure.Repositories.GlAccountImport;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.GlAccountImport
{
    // Exercises the real COA bulk-import commit against a live SQL test DB (GL02-FR-006):
    // single-transaction create of groups (Level/IsLeaf via the reused AccountGroup logic) +
    // accounts (Inactive, stamped with ImportLogId), the log header + row-error report, and the
    // bulk activate-batch flip.
    [Collection("DatabaseCollection")]
    public sealed class GlAccountImportCommandRepositoryTests
    {
        private const int CompanyId = 1;
        private readonly DbFixture _fixture;

        public GlAccountImportCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── seeding + commit helper ───────────────────────────────────────────
        private async Task<(int assetTypeId, int nbId, int sltId, int curId)> SeedMastersAsync()
        {
            using var ctx = _fixture.CreateFreshDbContext();

            var miscType = new MiscTypeMaster { MiscTypeCode = "NB", Description = "Normal Balance" };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var nb = new MiscMaster { MiscTypeId = miscType.Id, Code = "DR", Description = "Debit", SortOrder = 1 };
            var slt = new MiscMaster { MiscTypeId = miscType.Id, Code = "NONE", Description = "None", SortOrder = 2 };
            ctx.MiscMaster.AddRange(nb, slt);

            var atype = new AccountTypeMaster
            {
                CompanyId = CompanyId,
                AccountTypeName = "Asset",
                StartCode = "1",
                AccountCodeLength = 6,
                SortOrder = 1
            };
            ctx.AccountTypeMaster.Add(atype);

            // Fully-qualified: the bare name collides with the sibling test namespace
            // FinanceManagement.IntegrationTests.Repositories.CurrencyForexConfig.
            var cur = new FinanceManagement.Domain.Entities.CurrencyForexConfig
            {
                CompanyId = CompanyId,
                CurrencyTypeCode = "INRONLY",
                CurrencyTypeName = "INR Only"
            };
            ctx.CurrencyForexConfig.Add(cur);

            await ctx.SaveChangesAsync();
            return (atype.Id, nb.Id, slt.Id, cur.Id);
        }

        // Builds a request: root "1000" → child "1100" (leaf) with one account "110001",
        // plus one row-error to verify the report is retained.
        private static GlAccountImportCommitRequest BuildRequest(int assetTypeId, int nbId, int sltId, int curId)
            => new()
            {
                CompanyId = CompanyId,
                FileName = "coa.xlsx",
                FileFormat = "Excel",
                ImportMode = "ValidRowsOnly",
                TotalRows = 4,
                GroupRows = 2,
                AccountRows = 1,
                DurationMs = 15,
                Status = "CompletedWithSkips",
                Groups = new List<PlannedAccountGroup>
                {
                    new() { RowNumber = 2, GroupCode = "1000", GroupName = "Assets",
                            ParentGroupCode = null, ExistingParentId = null, AccountTypeId = assetTypeId, SortOrder = 1, Level = 1 },
                    new() { RowNumber = 3, GroupCode = "1100", GroupName = "Current Assets",
                            ParentGroupCode = "1000", ExistingParentId = null, AccountTypeId = null, SortOrder = 1, Level = 2 }
                },
                Accounts = new List<PlannedGlAccount>
                {
                    new() { RowNumber = 4, AccountCode = "110001", AccountName = "Cash", Description = "Petty cash",
                            AccountGroupCode = "1100", ExistingAccountGroupId = null,
                            AccountTypeId = assetTypeId, NormalBalanceId = nbId, CurrencyTypeId = curId, SubLedgerTypeId = sltId,
                            IsCostCentreMandatory = false, IsTaxRelevant = false, IsInterCompany = false, IsReconciliationRequired = false }
                },
                Errors = new List<GlAccountImportErrorDto>
                {
                    new() { RowNumber = 5, RecordType = "ACCOUNT", ColumnName = "AccountCode",
                            AttemptedValue = "99X", ErrorMessage = "AccountCode must contain digits only." }
                }
            };

        private async Task<int> CommitAsync(GlAccountImportCommitRequest request)
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var groupRepo = new AccountGroupCommandRepository(ctx);
            var importRepo = new GlAccountImportCommandRepository(ctx, groupRepo);
            return await importRepo.CommitAsync(request, CancellationToken.None);
        }

        private SqlConnection Conn() => new(_fixture.ConnectionString);

        // ── tests ─────────────────────────────────────────────────────────────
        [Fact]
        public async Task Commit_creates_groups_accounts_log_and_errors_with_accounts_inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var (assetTypeId, nbId, sltId, curId) = await SeedMastersAsync();

            var logId = await CommitAsync(BuildRequest(assetTypeId, nbId, sltId, curId));
            logId.Should().BeGreaterThan(0);

            await using var c = Conn();

            // Log header counters.
            var log = await c.QuerySingleAsync<LogRow>(
                @"SELECT ImportedGroups, ImportedAccounts, ValidRows, InvalidRows, SkippedRows, Status
                  FROM Finance.GlAccountImportLog WHERE Id = @logId", new { logId });
            log.ImportedGroups.Should().Be(2);
            log.ImportedAccounts.Should().Be(1);
            log.InvalidRows.Should().Be(1);
            log.ValidRows.Should().Be(3);          // TotalRows 4 − 1 invalid
            log.SkippedRows.Should().Be(1);
            log.Status.Should().Be("CompletedWithSkips");

            // Groups: root non-leaf @ level 1, child leaf @ level 2.
            var root = await c.QuerySingleAsync<GroupRow>(
                "SELECT [Level], IsLeaf, Id FROM Finance.AccountGroup WHERE GroupCode = '1000'");
            var child = await c.QuerySingleAsync<GroupRow>(
                "SELECT [Level], IsLeaf, Id FROM Finance.AccountGroup WHERE GroupCode = '1100'");
            root.Level.Should().Be(1);
            root.IsLeaf.Should().BeFalse("root gained a child group");
            child.Level.Should().Be(2);
            child.IsLeaf.Should().BeTrue("the bottom group is the leaf an account attaches to");

            // Account: Inactive, stamped with the import-log id, attached to the leaf group.
            var account = await c.QuerySingleAsync<AccountRow>(
                @"SELECT IsActive, ImportLogId, AccountGroupId
                  FROM Finance.GlAccountMaster WHERE AccountCode = '110001'");
            account.IsActive.Should().BeFalse("imported accounts default to Inactive (AC3)");
            account.ImportLogId.Should().Be(logId);
            account.AccountGroupId.Should().Be(child.Id);

            // Row-error report retained.
            var errorCount = await c.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Finance.GlAccountImportError WHERE ImportLogId = @logId", new { logId });
            errorCount.Should().Be(1);
        }

        [Fact]
        public async Task ActivateBatch_flips_only_that_batch_to_active()
        {
            await _fixture.ClearAllTablesAsync();
            var (assetTypeId, nbId, sltId, curId) = await SeedMastersAsync();
            var logId = await CommitAsync(BuildRequest(assetTypeId, nbId, sltId, curId));

            int activated;
            using (var ctx = _fixture.CreateFreshDbContext())
            {
                var importRepo = new GlAccountImportCommandRepository(ctx, new AccountGroupCommandRepository(ctx));
                activated = await importRepo.ActivateBatchAsync(logId, CompanyId, CancellationToken.None);
            }

            activated.Should().Be(1);

            await using var c = Conn();
            var isActive = await c.ExecuteScalarAsync<bool>(
                "SELECT IsActive FROM Finance.GlAccountMaster WHERE AccountCode = '110001'");
            isActive.Should().BeTrue("activate-batch flipped the imported account to Active");

            // Re-running is a no-op (already active).
            int again;
            using (var ctx = _fixture.CreateFreshDbContext())
            {
                var importRepo = new GlAccountImportCommandRepository(ctx, new AccountGroupCommandRepository(ctx));
                again = await importRepo.ActivateBatchAsync(logId, CompanyId, CancellationToken.None);
            }
            again.Should().Be(0, "nothing left to activate");
        }

        private sealed record LogRow(int ImportedGroups, int ImportedAccounts, int ValidRows, int InvalidRows, int SkippedRows, string Status);
        private sealed record GroupRow(int Level, bool IsLeaf, int Id);
        private sealed record AccountRow(bool IsActive, int? ImportLogId, int AccountGroupId);
    }
}
