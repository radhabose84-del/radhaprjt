using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;
using FinanceManagement.Infrastructure.Repositories.GlAccountImport;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.GlAccountImport
{
    // Query-side: reference-data load, code-based export rows (round-trips), and the
    // import-log / row-error history reads — against a live SQL test DB (GL02-FR-006).
    [Collection("DatabaseCollection")]
    public sealed class GlAccountImportQueryRepositoryTests
    {
        private const int CompanyId = 1;
        private readonly DbFixture _fixture;

        public GlAccountImportQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GlAccountImportQueryRepository QueryRepo()
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = CompanyId, CompanyName = "Test Co" } });
            return new GlAccountImportQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object);
        }

        // ── seeding + commit (drives realistic data through the command repo) ──
        private async Task<(int assetTypeId, int nbId, int sltId, int curId)> SeedMastersAsync()
        {
            using var ctx = _fixture.CreateFreshDbContext();

            // Normal-balance and sub-ledger-type live under distinct MiscTypes:
            // LoadReferenceDataAsync keys them by MiscTypeCode ("NB" vs "SLTYPE").
            var nbType = new MiscTypeMaster { MiscTypeCode = "NB", Description = "Normal Balance" };
            var sltType = new MiscTypeMaster { MiscTypeCode = "SLTYPE", Description = "Sub Ledger Type" };
            ctx.MiscTypeMaster.AddRange(nbType, sltType);
            await ctx.SaveChangesAsync();

            var nb = new MiscMaster { MiscTypeId = nbType.Id, Code = "DR", Description = "Debit", SortOrder = 1 };
            var slt = new MiscMaster { MiscTypeId = sltType.Id, Code = "NONE", Description = "None", SortOrder = 2 };
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

        private async Task<int> SeedAndCommitAsync()
        {
            var (assetTypeId, nbId, sltId, curId) = await SeedMastersAsync();
            using var ctx = _fixture.CreateFreshDbContext();
            var importRepo = new GlAccountImportCommandRepository(ctx, new AccountGroupCommandRepository(ctx));
            return await importRepo.CommitAsync(BuildRequest(assetTypeId, nbId, sltId, curId), CancellationToken.None);
        }

        // ── tests ─────────────────────────────────────────────────────────────
        [Fact]
        public async Task GetExportRows_returns_groups_then_accounts_in_code_form()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAndCommitAsync();

            var rows = await QueryRepo().GetExportRowsAsync(CompanyId, CancellationToken.None);

            // Groups (parent before child) precede accounts.
            var groupCodes = rows.Where(r => r.RecordType == "GROUP").Select(r => r.GroupCode).ToList();
            groupCodes.Should().ContainInOrder("1000", "1100");

            var account = rows.Single(r => r.RecordType == "ACCOUNT");
            account.AccountCode.Should().Be("110001");
            account.AccountGroupCode.Should().Be("1100", "accounts reference their group by code");
            account.NormalBalance.Should().Be("DR");
            account.Currency.Should().Be("INRONLY");
            account.SubLedgerType.Should().Be("NONE");

            // AccountType appears only on the Level-1 root group.
            rows.Single(r => r.GroupCode == "1000").AccountType.Should().Be("Asset");
            rows.Single(r => r.GroupCode == "1100").AccountType.Should().BeNull();
        }

        [Fact]
        public async Task GetLogs_returns_the_run_with_company_name()
        {
            await _fixture.ClearAllTablesAsync();
            var logId = await SeedAndCommitAsync();

            var (logs, total) = await QueryRepo().GetLogsAsync(CompanyId, 1, 20);

            total.Should().Be(1);
            var log = logs.Single();
            log.Id.Should().Be(logId);
            log.ImportedGroups.Should().Be(2);
            log.ImportedAccounts.Should().Be(1);
            log.CompanyName.Should().Be("Test Co", "populated via the company lookup");
        }

        [Fact]
        public async Task GetErrors_returns_the_retained_row_report()
        {
            await _fixture.ClearAllTablesAsync();
            var logId = await SeedAndCommitAsync();

            var errors = await QueryRepo().GetErrorsAsync(logId);

            errors.Should().ContainSingle();
            errors[0].RowNumber.Should().Be(5);
            errors[0].ColumnName.Should().Be("AccountCode");
            errors[0].ErrorMessage.Should().Contain("digits only");
        }

        [Fact]
        public async Task LogBelongsToCompany_guards_cross_company_access()
        {
            await _fixture.ClearAllTablesAsync();
            var logId = await SeedAndCommitAsync();

            (await QueryRepo().LogBelongsToCompanyAsync(logId, CompanyId)).Should().BeTrue();
            (await QueryRepo().LogBelongsToCompanyAsync(logId, 999)).Should().BeFalse();
        }

        [Fact]
        public async Task LoadReferenceData_loads_groups_types_codes_and_existing_accounts()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAndCommitAsync();

            var rd = await QueryRepo().LoadReferenceDataAsync(CompanyId, CancellationToken.None);

            rd.GroupsByCode.Keys.Should().Contain(new[] { "1000", "1100" });
            rd.AccountTypesByName.Should().ContainKey("ASSET");
            rd.NormalBalanceByCode.Should().ContainKey("DR");
            rd.CurrencyByCode.Should().ContainKey("INRONLY");
            rd.SubLedgerTypeByCode.Should().ContainKey("NONE");
            rd.ExistingAccountCodes.Should().Contain("110001");
        }
    }
}
