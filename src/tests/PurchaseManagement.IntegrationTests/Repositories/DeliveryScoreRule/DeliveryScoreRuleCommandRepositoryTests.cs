using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.DeliveryScoreRule;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.DeliveryScoreRule
{
    [Collection("DatabaseCollection")]
    public sealed class DeliveryScoreRuleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DeliveryScoreRuleCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DeliveryScoreRuleCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule BuildEntity(
            string ruleCode = "DSR001", string description = "On-time delivery") =>
            new()
            {
                RuleCode = ruleCode,
                Description = description,
                DelayDaysFrom = 0,
                DelayDaysTo = 0,
                Score = 100m,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DSR_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DSR_C2", "Late by 3 days"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DeliveryScoreRules.FirstAsync(x => x.Id == id);
            saved.RuleCode.Should().Be("DSR_C2");
            saved.Description.Should().Be("Late by 3 days");
            saved.Score.Should().Be(100m);
            saved.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DSR_U1"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("DSR_U1");
            entity.Id = id;
            entity.Description = "Updated";
            entity.Score = 75m;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.DeliveryScoreRules.FirstAsync(x => x.Id == id);
            reloaded.Description.Should().Be("Updated");
            reloaded.Score.Should().Be(75m);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_RuleCode()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DSR_IM"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("DSR_IM");
            entity.Id = id;
            entity.RuleCode = "CHANGED";
            entity.Description = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.DeliveryScoreRules.FirstAsync(x => x.Id == id);
            reloaded.RuleCode.Should().Be("DSR_IM");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity("DSR_GHOST");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DSR_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.DeliveryScoreRules.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
