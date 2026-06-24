using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    [Collection("DatabaseCollection")]
    public sealed class JournalQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalQueryRepository CreateQueryRepo()
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Test Company" } });

            var fy = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            fy.Setup(f => f.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>
                {
                    new() { FinancialYearId = 3, FinancialYearName = "2026-27", IsActive = true,
                            StartDate = new DateTime(2000, 1, 1), EndDate = new DateTime(2100, 1, 1) }
                });

            return new JournalQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object, fy.Object);
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedDraftAsync(SeededIds ids)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
        }

        [Fact]
        public async Task IsPostingEligibleAsync_ManualMustBeApproved_SystemDraftEligible()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var repo = CreateQueryRepo();

            // Manual DRAFT → not eligible (needs approval).
            int manualDraft;
            await using (var ctx = _fixture.CreateFreshDbContext())
                manualDraft = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
            (await repo.IsPostingEligibleAsync(manualDraft)).Should().BeFalse();

            // Manual APPROVED → eligible.
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var h = await ctx.JournalHeader.FirstAsync(x => x.Id == manualDraft);
                h.StatusId = ids.StatusApprovedId;
                await ctx.SaveChangesAsync();
            }
            (await repo.IsPostingEligibleAsync(manualDraft)).Should().BeTrue();

            // System (source != MANUAL) DRAFT → eligible (bypasses approval).
            int systemDraft;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var j = JournalTestSeed.BuildDraftJournal(ids);
                j.SourceId = ids.SourceRecurringId;
                systemDraft = await new JournalCommandRepository(ctx).CreateAsync(j);
            }
            (await repo.IsPostingEligibleAsync(systemDraft)).Should().BeTrue();
        }

        [Fact]
        public async Task GetPostableAsync_Returns_Only_Approved_And_System_Drafts()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Manual DRAFT → excluded (needs approval).
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            // Manual APPROVED → included.
            int approvedId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                approvedId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
                var h = await ctx.JournalHeader.FirstAsync(x => x.Id == approvedId);
                h.StatusId = ids.StatusApprovedId;
                await ctx.SaveChangesAsync();
            }

            // System (RECURRING) DRAFT → included (bypasses approval).
            int systemDraftId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var j = JournalTestSeed.BuildDraftJournal(ids);
                j.SourceId = ids.SourceRecurringId;
                systemDraftId = await new JournalCommandRepository(ctx).CreateAsync(j);
            }

            var (items, total) = await CreateQueryRepo().GetPostableAsync(1, 50, ids.CompanyId);

            total.Should().Be(2);
            items.Select(i => i.Id).Should().BeEquivalentTo(new[] { approvedId, systemDraftId });
        }

        [Fact]
        public async Task GetPendingApprovalAsync_Returns_Only_Manual_Drafts()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Manual DRAFT → included.
            int manualDraftId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                manualDraftId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            // Manual APPROVED → excluded (already approved).
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var approvedId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));
                var h = await ctx.JournalHeader.FirstAsync(x => x.Id == approvedId);
                h.StatusId = ids.StatusApprovedId;
                await ctx.SaveChangesAsync();
            }

            // System (RECURRING) DRAFT → excluded (not manual).
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var j = JournalTestSeed.BuildDraftJournal(ids);
                j.SourceId = ids.SourceRecurringId;
                await new JournalCommandRepository(ctx).CreateAsync(j);
            }

            var repo = CreateQueryRepo();
            var (items, total) = await repo.GetPendingApprovalAsync(1, 50, ids.CompanyId);

            total.Should().Be(1);
            items.Should().ContainSingle().Which.Id.Should().Be(manualDraftId);

            (await repo.IsManualDraftAsync(manualDraftId)).Should().BeTrue();
            (await repo.GetUnitIdAsync(manualDraftId)).Should().Be(1);
        }

        [Fact]
        public async Task GetStatusIdAsync_Should_Resolve_Draft()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var statusId = await CreateQueryRepo().GetStatusIdAsync("DRAFT");

            statusId.Should().Be(ids.StatusDraftId);
        }

        [Fact]
        public async Task SearchAsync_FiltersByAmountAndStatus()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 1000m));
                await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 5000m));
            }

            var repo = CreateQueryRepo();

            // No filter → both.
            var (all, allTotal) = await repo.SearchAsync(new FinanceManagement.Application.JournalMaster.Dto.JournalSearchFilter(), 1, 10, ids.CompanyId);
            all.Should().HaveCount(2);
            allTotal.Should().Be(2);

            // AmountMin → only the 5000 voucher.
            var (big, _) = await repo.SearchAsync(
                new FinanceManagement.Application.JournalMaster.Dto.JournalSearchFilter { AmountMin = 2000m }, 1, 10, ids.CompanyId);
            big.Should().ContainSingle().Which.TotalDr.Should().Be(5000m);

            // Status = Draft → both.
            var (drafts, _) = await repo.SearchAsync(
                new FinanceManagement.Application.JournalMaster.Dto.JournalSearchFilter { StatusId = ids.StatusDraftId }, 1, 10, ids.CompanyId);
            drafts.Should().HaveCount(2);

            // Line-level: account filter matches the debit GL account.
            var (byAccount, _) = await repo.SearchAsync(
                new FinanceManagement.Application.JournalMaster.Dto.JournalSearchFilter { AccountId = ids.GlAccountDrId }, 1, 10, ids.CompanyId);
            byAccount.Should().HaveCount(2);

            // Non-matching voucher no → none.
            var (none, noneTotal) = await repo.SearchAsync(
                new FinanceManagement.Application.JournalMaster.Dto.JournalSearchFilter { VoucherNo = "NOPE" }, 1, 10, ids.CompanyId);
            none.Should().BeEmpty();
            noneTotal.Should().Be(0);
        }

        [Fact]
        public async Task IsPotentialDuplicateAsync_DetectsMatchingVoucher_AndRespectsExclude()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var existingId = await SeedDraftAsync(ids);   // amount 1000, 2026-06-15

            var repo = CreateQueryRepo();
            var date = new DateOnly(2026, 6, 15);
            var sameLines = new List<(int, decimal, decimal)>
            {
                (ids.GlAccountDrId, 1000m, 0m),
                (ids.GlAccountCrId, 0m, 1000m)
            };

            // Identical signature → flagged as a potential duplicate.
            (await repo.IsPotentialDuplicateAsync(ids.CompanyId, ids.VoucherTypeId, date, 1000m, 1000m, sameLines, null))
                .Should().BeTrue();

            // Excluding the only candidate (e.g. updating itself) → not a duplicate.
            (await repo.IsPotentialDuplicateAsync(ids.CompanyId, ids.VoucherTypeId, date, 1000m, 1000m, sameLines, existingId))
                .Should().BeFalse();

            // Different amount → not a duplicate.
            var diffLines = new List<(int, decimal, decimal)>
            {
                (ids.GlAccountDrId, 2000m, 0m),
                (ids.GlAccountCrId, 0m, 2000m)
            };
            (await repo.IsPotentialDuplicateAsync(ids.CompanyId, ids.VoucherTypeId, date, 2000m, 2000m, diffLines, null))
                .Should().BeFalse();
        }

        [Fact]
        public async Task GetSourceIdAsync_Should_Resolve_Manual()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var sourceId = await CreateQueryRepo().GetSourceIdAsync("MANUAL");

            sourceId.Should().Be(ids.SourceManualId);
        }

        [Fact]
        public async Task GetOpenPeriodByDateAsync_Should_Resolve_Period_In_Range()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var period = await CreateQueryRepo().GetOpenPeriodByDateAsync(1, new DateOnly(2026, 6, 15));

            period.Should().NotBeNull();
            period!.Value.PeriodId.Should().Be(ids.AccountingPeriodId);
            period.Value.FinancialYearId.Should().Be(3);
        }

        [Fact]
        public async Task GetOpenPeriodByDateAsync_Should_Return_Null_OutOfRange()
        {
            await ClearTableAsync();
            await JournalTestSeed.SeedGraphAsync(_fixture);

            var period = await CreateQueryRepo().GetOpenPeriodByDateAsync(1, new DateOnly(2026, 9, 15));

            period.Should().BeNull();
        }

        [Fact]
        public async Task FkValidators_Should_Return_True_For_Seeded()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var repo = CreateQueryRepo();

            (await repo.VoucherTypeExistsAsync(ids.VoucherTypeId, 1)).Should().BeTrue();
            (await repo.GlAccountExistsAsync(ids.GlAccountDrId, 1)).Should().BeTrue();
            (await repo.CostCentreExistsAsync(ids.CostCentreId)).Should().BeTrue();
            (await repo.ProfitCentreExistsAsync(ids.ProfitCentreId)).Should().BeTrue();
            (await repo.CurrencyExistsAsync(ids.CurrencyId)).Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Lines()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var id = await SeedDraftAsync(ids);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Lines.Should().HaveCount(2);
            dto.CompanyName.Should().Be("Test Company");
            dto.FinancialYearName.Should().Be("2026-27");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Draft()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            await SeedDraftAsync(ids);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task State_Flags_Should_Reflect_Draft_Then_Posted()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var id = await SeedDraftAsync(ids);
            var repo = CreateQueryRepo();

            (await repo.IsDraftAsync(id)).Should().BeTrue();
            (await repo.IsBalancedAsync(id)).Should().BeTrue();
            (await repo.IsPeriodOpenAsync(id)).Should().BeTrue();
            (await repo.IsPostedAsync(id)).Should().BeFalse();

            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(id, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            (await repo.IsPostedAsync(id)).Should().BeTrue();
            (await repo.IsDraftAsync(id)).Should().BeFalse();
        }
    }
}
