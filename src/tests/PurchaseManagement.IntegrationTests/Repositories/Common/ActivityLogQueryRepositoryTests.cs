using Microsoft.EntityFrameworkCore;

namespace PurchaseManagement.IntegrationTests.Repositories.Common
{
    [Collection("DatabaseCollection")]
    public sealed class ActivityLogQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ActivityLogQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ActivityLogQueryRepository CreateRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static PurchaseManagement.Domain.Entities.ActivityLog BuildLog(
            string entityName,
            int entityId,
            string action = "Update",
            string propertyName = "SomeProperty",
            string oldValue = "OldVal",
            string newValue = "NewVal",
            DateTimeOffset? createdDate = null) =>
            new()
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedDate = createdDate ?? DateTimeOffset.UtcNow,
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        private async Task SeedAsync(params PurchaseManagement.Domain.Entities.ActivityLog[] logs)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.ActivityLogs.AddRange(logs);
            await ctx.SaveChangesAsync();
        }

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoMatch()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateRepo(ctx)
                .GetAllAsync("PurchaseOrder", 99999, 1, 10, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_EntityName_And_EntityId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync(
                BuildLog("PurchaseOrder", 1),
                BuildLog("PurchaseOrder", 2),
                BuildLog("Quotation", 1));

            await using var ctx = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateRepo(ctx)
                .GetAllAsync("PurchaseOrder", 1, 1, 10, CancellationToken.None);

            items.Should().ContainSingle();
            total.Should().Be(1);
            items[0].EntityName.Should().Be("PurchaseOrder");
            items[0].EntityId.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Order_By_CreatedDate_Descending()
        {
            await _fixture.ClearAllTablesAsync();
            var older = DateTimeOffset.UtcNow.AddHours(-2);
            var newer = DateTimeOffset.UtcNow;
            await SeedAsync(
                BuildLog("Indent", 5, propertyName: "Older", createdDate: older),
                BuildLog("Indent", 5, propertyName: "Newer", createdDate: newer));

            await using var ctx = _fixture.CreateFreshDbContext();
            var (items, _) = await CreateRepo(ctx)
                .GetAllAsync("Indent", 5, 1, 10, CancellationToken.None);

            items.Should().HaveCount(2);
            items[0].PropertyName.Should().Be("Newer");
            items[1].PropertyName.Should().Be("Older");
        }

        [Fact]
        public async Task GetAllAsync_Should_Apply_Pagination()
        {
            await _fixture.ClearAllTablesAsync();
            for (int i = 0; i < 5; i++)
                await SeedAsync(BuildLog("Quotation", 100, propertyName: $"Prop{i}",
                                          createdDate: DateTimeOffset.UtcNow.AddMinutes(-i)));

            await using var ctx = _fixture.CreateFreshDbContext();
            var (page1, total) = await CreateRepo(ctx)
                .GetAllAsync("Quotation", 100, 1, 2, CancellationToken.None);
            var (page2, _) = await CreateRepo(ctx)
                .GetAllAsync("Quotation", 100, 2, 2, CancellationToken.None);

            total.Should().Be(5);
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
            page1.Select(x => x.PropertyName).Should().NotIntersectWith(page2.Select(x => x.PropertyName));
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdAsync(99999L, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Log()
        {
            await _fixture.ClearAllTablesAsync();
            var log = BuildLog("PurchaseOrder", 42, propertyName: "Status");
            await SeedAsync(log);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdAsync(log.Id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(log.Id);
            result.EntityName.Should().Be("PurchaseOrder");
            result.PropertyName.Should().Be("Status");
        }
    }
}
