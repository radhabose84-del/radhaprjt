using Microsoft.EntityFrameworkCore;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Services;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule;

namespace FinanceManagement.IntegrationTests.Repositories.JournalThresholdRule
{
    // US-GL01-03B — end-to-end gap scan against the real DB: post a continuous run of vouchers (no gap),
    // then introduce a gap by advancing the number series past the assigned vouchers and re-scan.
    [Collection("DatabaseCollection")]
    public sealed class GapScanServiceTests
    {
        private readonly DbFixture _fixture;

        public GapScanServiceTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private GapScanService CreateService(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            return new GapScanService(new GapScanRepository(ctx), tz.Object);
        }

        private async Task<int> CreateAndPostAsync(SeededIds ids, string fyName, int userId)
        {
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(id, ids.StatusPostedId, fyName, "Tester", userId, DateTimeOffset.UtcNow, CancellationToken.None);

            return id;
        }

        [Fact]
        public async Task ContinuousRun_FindsNoGaps_AndLogsCleanScan()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            await CreateAndPostAsync(ids, "2026-27", 1);   // JV/2026-27/0001
            await CreateAndPostAsync(ids, "2026-27", 1);   // JV/2026-27/0002

            int total;
            await using (var ctx = _fixture.CreateFreshDbContext())
                total = await CreateService(ctx).ScanAllAsync(CancellationToken.None);

            total.Should().Be(0);

            await using var verify = _fixture.CreateFreshDbContext();
            var log = await verify.SequenceGapScanLog.OrderByDescending(l => l.Id).FirstAsync();
            log.GapsFound.Should().Be(0);
            log.Alerted.Should().BeFalse();
            log.GapNumbers.Should().BeNull();
        }

        [Fact]
        public async Task AdvancedSeries_BeyondAssignedVouchers_DetectsGap_AndAlerts()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            await CreateAndPostAsync(ids, "2026-27", 1);   // JV/2026-27/0001
            await CreateAndPostAsync(ids, "2026-27", 1);   // JV/2026-27/0002

            // Advance the series to 4 without assigning vouchers 3 & 4 → simulates a sequence gap.
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var series = await ctx.VoucherTypeNumberSeries
                    .FirstAsync(s => s.VoucherTypeId == ids.VoucherTypeId && s.FinancialYearId == ids.FinancialYearId);
                series.LastUsedNumber = 4;
                await ctx.SaveChangesAsync();
            }

            int total;
            await using (var ctx = _fixture.CreateFreshDbContext())
                total = await CreateService(ctx).ScanAllAsync(CancellationToken.None);

            total.Should().Be(2);

            await using var verify = _fixture.CreateFreshDbContext();
            var log = await verify.SequenceGapScanLog.OrderByDescending(l => l.Id).FirstAsync();
            log.GapsFound.Should().Be(2);
            log.GapNumbers.Should().Be("3,4");
            log.Alerted.Should().BeTrue();
        }
    }
}
