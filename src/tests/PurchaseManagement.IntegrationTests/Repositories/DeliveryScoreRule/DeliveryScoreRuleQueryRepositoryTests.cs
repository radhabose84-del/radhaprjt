using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.DeliveryScoreRule;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.DeliveryScoreRule
{
    [Collection("DatabaseCollection")]
    public sealed class DeliveryScoreRuleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DeliveryScoreRuleQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DeliveryScoreRuleQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string ruleCode, string description = "desc",
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new PurchaseManagement.Domain.Entities.VendorEvaluation.DeliveryScoreRule
            {
                RuleCode = ruleCode,
                Description = description,
                DelayDaysFrom = 0,
                DelayDaysTo = 0,
                Score = 100m,
                SortOrder = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.DeliveryScoreRules.AddAsync(e);
            await ctx.SaveChangesAsync();

            // IsActive default-value guard: Status.Inactive is CLR default → force via follow-up update.
            if (active == Status.Inactive)
            {
                e.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }
            return e.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("DSRQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DSRQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("DSRQ_UNIQ");
            await SeedAsync("DSRQ_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "DSRQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].RuleCode.Should().Be("DSRQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DSRQ_GBI", "lookup me");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.RuleCode.Should().Be("DSRQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("DSRQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("DSRQ_AC1", "Active rule");
            await SeedAsync("DSRQ_AC2", "Inactive rule", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("DSRQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].RuleCode.Should().Be("DSRQ_AC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("DSRQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("DSRQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("DSRQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("DSRQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }
    }
}
