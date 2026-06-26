using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    /// <summary>
    /// US-GL03-04 — exercises the persisted-computed IsBackdated column + the late-posting
    /// report Dapper query against the live test DB. Critical: the computed column is the
    /// security guarantee that backdating cannot be hidden by a client-side flag-flip.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class JournalLatePostingReportTests
    {
        private readonly DbFixture _fixture;

        public JournalLatePostingReportTests(DbFixture fixture)
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

        // Posts a journal with explicit VoucherDate and PostedAt so the IsBackdated computed column
        // fires deterministically. Calling PostAsync sets PostedAt = postedAt argument.
        private async Task<int> SeedPostedJournalAsync(
            SeededIds ids, DateOnly voucherDate, DateTimeOffset postedAt)
        {
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var draft = JournalTestSeed.BuildDraftJournal(ids);
                draft.VoucherDate = voucherDate;
                id = await new JournalCommandRepository(ctx).CreateAsync(draft);
            }

            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(
                    id, ids.StatusPostedId, "2026-27", "Tester", 1, postedAt, CancellationToken.None);

            return id;
        }

        // ─── Computed column behaviour ──────────────────────────────────────

        [Fact]
        public async Task IsBackdated_Should_Be_True_When_VoucherDate_Lt_PostedAt_Date()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var id = await SeedPostedJournalAsync(
                ids,
                voucherDate: new DateOnly(2026, 6, 1),
                postedAt:    new DateTimeOffset(2026, 6, 10, 9, 0, 0, TimeSpan.Zero));

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.JournalHeader.AsNoTracking().FirstAsync(h => h.Id == id);
            header.IsBackdated.Should().BeTrue();
        }

        [Fact]
        public async Task IsBackdated_Should_Be_False_When_VoucherDate_Equals_PostedAt_Date()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var sameDay = new DateOnly(2026, 6, 15);
            var id = await SeedPostedJournalAsync(
                ids, sameDay,
                new DateTimeOffset(sameDay.ToDateTime(TimeOnly.MinValue).AddHours(10), TimeSpan.Zero));

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.JournalHeader.AsNoTracking().FirstAsync(h => h.Id == id);
            header.IsBackdated.Should().BeFalse();
        }

        [Fact]
        public async Task IsBackdated_Should_Be_False_When_PostedAt_Is_Null_Draft()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var draft = JournalTestSeed.BuildDraftJournal(ids);
                draft.VoucherDate = new DateOnly(2026, 5, 1);   // would be backdated if posted today
                id = await new JournalCommandRepository(ctx).CreateAsync(draft);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var header = await verify.JournalHeader.AsNoTracking().FirstAsync(h => h.Id == id);
            header.IsBackdated.Should().BeFalse();   // not posted yet → not backdated
        }

        // ─── Late-posting report query ─────────────────────────────────────

        [Fact]
        public async Task GetLatePostingReport_Should_Return_Only_Backdated_Posted_Vouchers()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Backdated (May 1 voucher posted June 10) — should appear.
            var backdated = await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1), new DateTimeOffset(2026, 6, 10, 9, 0, 0, TimeSpan.Zero));

            // On-time (same-day) — should NOT appear.
            await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 15),
                new DateTimeOffset(2026, 6, 15, 14, 0, 0, TimeSpan.Zero));

            var (rows, total) = await CreateQueryRepo().GetLatePostingReportAsync(
                1, 50, 1, accountingPeriodId: null, fromDate: null, toDate: null,
                sortBy: null, sortDirection: null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
            rows[0].Id.Should().Be(backdated);
            rows[0].IsBackdated.Should().BeTrue();
            rows[0].DaysBackdated.Should().Be(9);
        }

        [Fact]
        public async Task GetLatePostingReport_Populates_CompanyName_FromLookup()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1),
                new DateTimeOffset(2026, 6, 5, 9, 0, 0, TimeSpan.Zero));

            var (rows, _) = await CreateQueryRepo().GetLatePostingReportAsync(
                1, 50, 1, null, null, null, null, null);

            rows.Should().HaveCount(1);
            rows[0].CompanyName.Should().Be("Test Company");
        }

        [Fact]
        public async Task GetLatePostingReport_Should_Filter_By_PostedAt_DateRange()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // Inside window.
            var inside = await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1), new DateTimeOffset(2026, 6, 15, 9, 0, 0, TimeSpan.Zero));

            // Outside window (before FromDate).
            await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 5, 1), new DateTimeOffset(2026, 5, 10, 9, 0, 0, TimeSpan.Zero));

            var (rows, _) = await CreateQueryRepo().GetLatePostingReportAsync(
                1, 50, 1, null,
                fromDate: new DateOnly(2026, 6, 10), toDate: new DateOnly(2026, 6, 30),
                sortBy: null, sortDirection: null);

            rows.Should().HaveCount(1);
            rows[0].Id.Should().Be(inside);
        }

        [Fact]
        public async Task GetLatePostingReport_Sort_DaysBackdated_DESC_OrdersByDaysLate()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var small = await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 14), new DateTimeOffset(2026, 6, 15, 9, 0, 0, TimeSpan.Zero));    // 1 day
            var big   = await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1),  new DateTimeOffset(2026, 6, 30, 9, 0, 0, TimeSpan.Zero));    // 29 days

            var (rows, _) = await CreateQueryRepo().GetLatePostingReportAsync(
                1, 50, 1, null, null, null, sortBy: "DaysBackdated", sortDirection: "DESC");

            rows.Should().HaveCount(2);
            rows[0].Id.Should().Be(big);
            rows[1].Id.Should().Be(small);
        }

        // ─── Weekly digest helper ──────────────────────────────────────────

        [Fact]
        public async Task GetBackdatedJournalsForDigest_Returns_Only_Window()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            var inWindow = await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1),
                new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero));

            // Outside window: posted Aug 1 — beyond July 1 cutoff.
            await SeedPostedJournalAsync(
                ids, new DateOnly(2026, 6, 1),
                new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.Zero));

            var rows = await CreateQueryRepo().GetBackdatedJournalsForDigestAsync(
                companyId: 1,
                windowStartUtc: new DateTimeOffset(2026, 6, 20, 0, 0, 0, TimeSpan.Zero),
                windowEndUtc:   new DateTimeOffset(2026, 7, 1,  0, 0, 0, TimeSpan.Zero),
                ct: CancellationToken.None);

            rows.Should().HaveCount(1);
            rows[0].Id.Should().Be(inWindow);
        }
    }
}
