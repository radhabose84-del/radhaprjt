using Infrastructure.Persistence.Repositories;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.IntegrationTests.Repositories.Budget
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetLogQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetLogQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetLogQueryRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        [Fact]
        public async Task GetLogsAsync_Should_Return_Empty_When_NoLogs()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).GetLogsAsync(null, null);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLogsAsync_Should_Return_Empty_When_BudgetId_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).GetLogsAsync(budgetId: 9999, budgetDetailId: null);

            result.Should().BeEmpty();
        }
    }
}
