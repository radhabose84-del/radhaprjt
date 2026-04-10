namespace BudgetManagement.IntegrationTests.Repositories.ActivityLog
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityLogQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityLogQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityLogQueryRepository CreateRepository()
        {
            var ctx = _fixture.CreateFreshDbContext();
            return new ActivityLogQueryRepository(ctx);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_No_Data()
        {
            var repo = CreateRepository();
            var (items, total) = await repo.GetAllAsync("NonExistentEntity", 99999, 1, 10, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var repo = CreateRepository();
            var result = await repo.GetByIdAsync(99999, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
