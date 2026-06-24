using Microsoft.Data.SqlClient;
using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.JournalThresholdRule
{
    [Collection("DatabaseCollection")]
    public sealed class JournalThresholdRuleRepositoryTests
    {
        private readonly DbFixture _fixture;

        public JournalThresholdRuleRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private JournalThresholdRuleCommandRepository CreateCommandRepo(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);
        private JournalThresholdRuleQueryRepository CreateQueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // Seeds a THRESHOLD_RULE_TYPE MiscMaster row and returns its id.
        private async Task<int> SeedRuleTypeAsync(string code = "AMT_OVER", string description = "Amount Over")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = new MiscTypeMaster { MiscTypeCode = "THRESHOLD_RULE_TYPE", Description = "Threshold Rule Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();
            var misc = new MiscMaster { MiscTypeId = type.Id, Code = code, Description = description, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private static FinanceManagement.Domain.Entities.JournalThresholdRule BuildRule(int ruleTypeId, decimal? value = 5000000m) =>
            new()
            {
                RuleTypeId = ruleTypeId,
                ThresholdValue = value,
                Active = true,
                EffectiveFrom = new DateOnly(2026, 4, 1),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Persist_Rule()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedRuleTypeAsync();

            int newId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                newId = await CreateCommandRepo(ctx).CreateAsync(BuildRule(ruleTypeId));

            newId.Should().BeGreaterThan(0);

            var dto = await CreateQueryRepo().GetByIdAsync(newId);
            dto.Should().NotBeNull();
            dto!.RuleTypeName.Should().Be("Amount Over");
            dto.ThresholdValue.Should().Be(5000000m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedRuleTypeAsync();
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateCommandRepo(ctx).CreateAsync(BuildRule(ruleTypeId));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = BuildRule(ruleTypeId, 9000000m);
                entity.Id = id;
                entity.Active = false;
                await CreateCommandRepo(ctx).UpdateAsync(entity);
            }

            var dto = await CreateQueryRepo().GetByIdAsync(id);
            dto!.ThresholdValue.Should().Be(9000000m);
            dto.Active.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Hide_From_GetAll()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedRuleTypeAsync();
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateCommandRepo(ctx).CreateAsync(BuildRule(ruleTypeId));

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);
            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task RuleTypeExistsAsync_Should_Return_True_For_Seeded()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedRuleTypeAsync();

            (await CreateQueryRepo().RuleTypeExistsAsync(ruleTypeId)).Should().BeTrue();
            (await CreateQueryRepo().RuleTypeExistsAsync(99999)).Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Rule()
        {
            await ClearTableAsync();
            var ruleTypeId = await SeedRuleTypeAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).CreateAsync(BuildRule(ruleTypeId));

            var results = await CreateQueryRepo().AutocompleteAsync("Amount", CancellationToken.None);
            results.Should().ContainSingle(r => r.RuleTypeName == "Amount Over");
        }

        [Fact]
        public async Task GetFlagsAsync_Should_Return_Seeded_Flag()
        {
            await ClearTableAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);
            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids));

            var ruleTypeId = await SeedRuleTypeAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.JournalFlag.Add(new JournalFlag
                {
                    JournalHeaderId = journalId,
                    RuleTypeId = ruleTypeId,
                    Value = 2500000m,
                    FlaggedAt = DateTimeOffset.UtcNow,
                    DigestSent = false
                });
                await ctx.SaveChangesAsync();
            }

            var (flags, total) = await CreateQueryRepo().GetFlagsAsync(1, 10, journalId);
            total.Should().Be(1);
            flags.Should().ContainSingle(f => f.JournalHeaderId == journalId && f.RuleTypeName == "Amount Over");
        }
    }
}
