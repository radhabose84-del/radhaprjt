using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.LogAnalysis;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.LogAnalysis
{
    [Collection("DatabaseCollection")]
    public sealed class LogAnalysisQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LogAnalysisQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LogAnalysisQueryRepository CreateRepo() => new(new SqlConnection(_fixture.ConnectionString));

        [Fact]
        public async Task GetAll_And_Summary_Normalize_Across_Log_Sources()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            // A posted journal → JournalHeader (for flag/violation refs) + a VoucherTypeNumberSeries (for gap).
            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids), "2026-27", 1);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(journalId, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            int seriesId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                seriesId = (await ctx.VoucherTypeNumberSeries.AsNoTracking().FirstAsync()).Id;

                ctx.SecurityViolationLog.Add(new FinanceManagement.Domain.Entities.SecurityViolationLog
                {
                    TableName = "JournalHeader", JournalHeaderId = journalId, AttemptedAction = "UPDATE",
                    UserName = "intruder", AttemptedAt = DateTimeOffset.UtcNow, Channel = "DB"
                });
                ctx.JournalFlag.Add(new JournalFlag
                {
                    JournalHeaderId = journalId, RuleTypeId = ids.StatusDraftId, Value = 250000m,
                    FlaggedAt = DateTimeOffset.UtcNow, DigestSent = false
                });
                ctx.SequenceGapScanLog.Add(new SequenceGapScanLog
                {
                    SeriesId = seriesId, ScannedAt = DateTimeOffset.UtcNow, GapsFound = 2, GapNumbers = "3,4", Alerted = true
                });
                await ctx.SaveChangesAsync();
            }

            var repo = CreateRepo();

            // GetAll executes the FULL 4-branch union against the real schema.
            var (rows, total) = await repo.GetAllAsync(null, null, null, 1, 50);
            total.Should().Be(3);
            rows.Select(r => r.LogType).Should().BeEquivalentTo(new[] { "SecurityViolation", "JournalFlag", "SequenceGap" });
            rows.Single(r => r.LogType == "SequenceGap").Summary.Should().Contain("gap(s) found");

            // Type filter.
            var (flagsOnly, flagTotal) = await repo.GetAllAsync("JournalFlag", null, null, 1, 50);
            flagTotal.Should().Be(1);
            flagsOnly.Should().ContainSingle().Which.Summary.Should().Contain("flagged");

            // Summary counts (RecurringGeneration branch runs but has 0 rows).
            var summary = await repo.GetSummaryAsync(null, null);
            summary.SecurityViolationCount.Should().Be(1);
            summary.JournalFlagCount.Should().Be(1);
            summary.SequenceGapCount.Should().Be(1);
            summary.RecurringGenerationCount.Should().Be(0);
            summary.TotalCount.Should().Be(3);
        }
    }
}
