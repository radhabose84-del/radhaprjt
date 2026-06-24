using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.JournalThresholdRule
{
    [Collection("DatabaseCollection")]
    public sealed class JournalFlagEngineRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalFlagEngineRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalFlagEngineRepository CreateRepo(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedAmountOverRuleAsync(decimal threshold = 5000000m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = new MiscTypeMaster { MiscTypeCode = "THRESHOLD_RULE_TYPE", Description = "Threshold Rule Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();

            var misc = new MiscMaster { MiscTypeId = type.Id, Code = "AMT_OVER", Description = "Amount Over", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();

            ctx.JournalThresholdRule.Add(new FinanceManagement.Domain.Entities.JournalThresholdRule
            {
                RuleTypeId = misc.Id, ThresholdValue = threshold, Active = true, EffectiveFrom = new DateOnly(2026, 4, 1),
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        [Fact]
        public async Task GetActiveThresholdRulesAsync_Should_Return_Active_Rule_With_Code()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedAmountOverRuleAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var rules = await CreateRepo(ctx).GetActiveThresholdRulesAsync(CancellationToken.None);

            rules.Should().ContainSingle();
            rules[0].RuleTypeId.Should().Be(ruleTypeId);
            rules[0].RuleTypeCode.Should().Be("AMT_OVER");
            rules[0].ThresholdValue.Should().Be(5000000m);
        }

        [Fact]
        public async Task AddFlags_Then_Digest_Cycle_Works()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            var ruleTypeId = await SeedAmountOverRuleAsync();

            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            // Raise a flag.
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepo(ctx).AddFlagsAsync(new[]
                {
                    new JournalFlag { JournalHeaderId = journalId, RuleTypeId = ruleTypeId, Value = 6000000m, FlaggedAt = DateTimeOffset.UtcNow, DigestSent = false }
                }, CancellationToken.None);

            // It shows up as undigested.
            int flagId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var undigested = await CreateRepo(ctx).GetUndigestedFlagsAsync(CancellationToken.None);
                undigested.Should().ContainSingle();
                flagId = undigested[0].Id;
            }

            // Mark it sent → no longer undigested.
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepo(ctx).MarkDigestSentAsync(new[] { flagId }, CancellationToken.None);

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await CreateRepo(ctx).GetUndigestedFlagsAsync(CancellationToken.None)).Should().BeEmpty();

            await using var verify = _fixture.CreateFreshDbContext();
            (await verify.JournalFlag.FirstAsync(f => f.Id == flagId)).DigestSent.Should().BeTrue();
        }
    }
}
