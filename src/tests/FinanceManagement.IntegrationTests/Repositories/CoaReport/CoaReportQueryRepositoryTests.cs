using FinanceManagement.Infrastructure.Repositories.CoaReport;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using Microsoft.Data.SqlClient;
using Entities = FinanceManagement.Domain.Entities;

namespace FinanceManagement.IntegrationTests.Repositories.CoaReport
{
    // US-GL02-15 — COA listing/usage/FS-mapping aggregates against the real test DB. Reuses
    // JournalTestSeed for the posted-journal graph (status/source/voucher type/period/currency),
    // then layers Schedule III mapping + report-specific accounts + ledger balances on top.
    [Collection("DatabaseCollection")]
    public sealed class CoaReportQueryRepositoryTests
    {
        private const int CompanyId = 1;
        private readonly DbFixture _fixture;
        public CoaReportQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CoaReportQueryRepository Repo() => new(new SqlConnection(_fixture.ConnectionString));

        private sealed class Seed
        {
            public int RecentAccId;
            public int StaleBsWithBalanceAccId;
            public int NeverPostedAccId;
            public string UnmappedGroupCode = "";
        }

        private async Task<Seed> SeedAsync()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var seed = new Seed();

            // Misc lookups: NB/SLT for accounts + Schedule III statement type 'BS' + a nature.
            var rptType = new Entities.MiscTypeMaster { MiscTypeCode = "RPT_MISC", Description = "RPT", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var s3Type = new Entities.MiscTypeMaster { MiscTypeCode = "S3_STMT_TYPE", Description = "S3", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.AddRange(rptType, s3Type);
            await ctx.SaveChangesAsync();

            var nb = new Entities.MiscMaster { MiscTypeId = rptType.Id, Code = "NB", Description = "Normal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var slt = new Entities.MiscMaster { MiscTypeId = rptType.Id, Code = "SLT", Description = "SubLedger", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var nature = new Entities.MiscMaster { MiscTypeId = rptType.Id, Code = "NAT", Description = "Asset", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var bs = new Entities.MiscMaster { MiscTypeId = s3Type.Id, Code = "BS", Description = "Balance Sheet", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.AddRange(nb, slt, nature, bs);
            await ctx.SaveChangesAsync();

            var section = new Entities.ScheduleIIISection { SectionName = "Current Assets", StatementTypeId = bs.Id, NatureId = nature.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<Entities.ScheduleIIISection>().Add(section);
            await ctx.SaveChangesAsync();

            var lineItem = new Entities.ScheduleIIISectionItem { SectionId = section.Id, LineCode = "A1", LineName = "Cash & Cash Equivalents", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.Set<Entities.ScheduleIIISectionItem>().Add(lineItem);
            await ctx.SaveChangesAsync();

            var accType = new Entities.AccountTypeMaster { CompanyId = CompanyId, AccountTypeName = "Rpt Asset", StartCode = "9", AccountCodeLength = 7, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.AccountTypeMaster.Add(accType);
            var currency = new Entities.CurrencyForexConfig { CompanyId = CompanyId, CurrencyTypeCode = "INRR", CurrencyTypeName = "Rupee Rpt", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.CurrencyForexConfig.Add(currency);
            await ctx.SaveChangesAsync();

            var mappedLeaf = new Entities.AccountGroup { CompanyId = CompanyId, GroupCode = "RPTBS", GroupName = "Mapped BS Leaf", Level = 1, IsLeaf = true, SortOrder = 1, ScheduleIIISectionItemId = lineItem.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var unmappedLeaf = new Entities.AccountGroup { CompanyId = CompanyId, GroupCode = "RPTUN", GroupName = "Unmapped Leaf", Level = 1, IsLeaf = true, SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.AccountGroup.AddRange(mappedLeaf, unmappedLeaf);
            await ctx.SaveChangesAsync();
            seed.UnmappedGroupCode = unmappedLeaf.GroupCode;

            Entities.GlAccountMaster Acc(string code, string name, int groupId) => new()
            {
                CompanyId = CompanyId, AccountTypeId = accType.Id, AccountGroupId = groupId,
                AccountCode = code, AccountName = name,
                NormalBalanceId = nb.Id, CurrencyTypeId = currency.Id, SubLedgerTypeId = slt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var recent = Acc("9100001", "Recently Posted", mappedLeaf.Id);
            var staleBs = Acc("9100002", "Stale BS w/ Balance", mappedLeaf.Id);
            var never = Acc("9100003", "Never Posted", unmappedLeaf.Id);
            ctx.GlAccountMaster.AddRange(recent, staleBs, never);
            await ctx.SaveChangesAsync();
            seed.RecentAccId = recent.Id;
            seed.StaleBsWithBalanceAccId = staleBs.Id;
            seed.NeverPostedAccId = never.Id;

            // Posted journals: recent (-1 month) and stale (-14 months).
            var today = DateOnly.FromDateTime(DateTime.Today);
            ctx.Set<Entities.JournalHeader>().Add(PostedJournal(ids, recent.Id, today.AddMonths(-1), currency.Id));
            ctx.Set<Entities.JournalHeader>().Add(PostedJournal(ids, staleBs.Id, today.AddMonths(-14), currency.Id));
            await ctx.SaveChangesAsync();

            // Non-zero balance for the stale BS account → must be EXCLUDED from deactivation candidates (AC3).
            ctx.Set<Entities.LedgerBalance>().Add(new Entities.LedgerBalance
            {
                CompanyId = CompanyId, GlAccountId = staleBs.Id, AccountingPeriodId = ids.AccountingPeriodId,
                FinancialYearId = ids.FinancialYearId, DrTotal = 1000m, CrTotal = 0m, Balance = 1000m
            });
            await ctx.SaveChangesAsync();

            return seed;
        }

        private static Entities.JournalHeader PostedJournal(SeededIds ids, int glAccountId, DateOnly postingDate, int currencyId) => new()
        {
            CompanyId = CompanyId, UnitId = 1, VoucherTypeId = ids.VoucherTypeId,
            VoucherDate = postingDate, PostingDate = postingDate,
            FinancialYearId = ids.FinancialYearId, AccountingPeriodId = ids.AccountingPeriodId,
            StatusId = ids.StatusPostedId, SourceId = ids.SourceManualId,
            TotalDr = 1000m, TotalCr = 1000m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
            Details = new List<Entities.JournalDetail>
            {
                new() { LineNo = 1, GlAccountId = glAccountId, DrAmount = 1000m, CrAmount = 0m, CurrencyId = currencyId,
                        ExchangeRate = 1m, BaseDrAmount = 1000m, BaseCrAmount = 0m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                new() { LineNo = 2, GlAccountId = ids.GlAccountCrId, DrAmount = 0m, CrAmount = 1000m, CurrencyId = currencyId,
                        ExchangeRate = 1m, BaseDrAmount = 0m, BaseCrAmount = 1000m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            }
        };

        // ── AC1 listing ───────────────────────────────────────────────────────────
        [Fact]
        public async Task Listing_CarriesPostingCount_AndFsMapping()
        {
            var seed = await SeedAsync();
            var rows = await Repo().GetCoaListingAsync(CompanyId, null, null, false, null, CancellationToken.None);

            var recent = rows.Single(r => r.Id == seed.RecentAccId);
            recent.PostingCount.Should().BeGreaterThan(0);
            recent.StatementTypeCode.Should().Be("BS");
            recent.ScheduleIIISectionItemId.Should().NotBeNull();

            var never = rows.Single(r => r.Id == seed.NeverPostedAccId);
            never.PostingCount.Should().Be(0);
            never.ScheduleIIISectionItemId.Should().BeNull("its group is unmapped");
        }

        // ── AC2/AC3 usage ─────────────────────────────────────────────────────────
        [Fact]
        public async Task Usage_ExcludesRecent_FlagsCandidates_AndProtectsBsWithBalance()
        {
            var seed = await SeedAsync();
            var rows = await Repo().GetAccountUsageAsync(CompanyId, 12, CancellationToken.None);

            rows.Should().NotContain(r => r.Id == seed.RecentAccId, "it was posted within the 12-month window");

            var staleBs = rows.Single(r => r.Id == seed.StaleBsWithBalanceAccId);
            staleBs.IsBalanceSheet.Should().BeTrue();
            staleBs.Balance.Should().Be(1000m);
            staleBs.IsDeactivationCandidate.Should().BeFalse("AC3 — BS account with a non-zero balance");
            staleBs.ExclusionReason.Should().NotBeNullOrEmpty();

            var never = rows.Single(r => r.Id == seed.NeverPostedAccId);
            never.NeverPosted.Should().BeTrue();
            never.IsDeactivationCandidate.Should().BeTrue();
        }

        // ── AC4 FS-mapping validation ───────────────────────────────────────────────
        [Fact]
        public async Task FsMappingValidation_ListsUnmappedLeafGroups()
        {
            var seed = await SeedAsync();
            var result = await Repo().GetFsMappingValidationAsync(CompanyId, CancellationToken.None);

            result.IsClean.Should().BeFalse();
            result.UnmappedCount.Should().BeGreaterThan(0);
            result.Unmapped.Should().Contain(u => u.GroupCode == seed.UnmappedGroupCode);
            result.MappedCount.Should().BeGreaterThan(0, "the mapped BS leaf is counted as mapped");
        }
    }
}
