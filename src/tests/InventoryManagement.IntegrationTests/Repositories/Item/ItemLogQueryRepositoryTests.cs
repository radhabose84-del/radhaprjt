using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemLogQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemLogQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemLogQueryRepository CreateRepo() =>
            new(_fixture.CreateFreshDbContext());

        private async Task<long> SeedLogAsync(
            string entityName = "Item",
            int entityId = 1,
            string propertyName = "Name",
            string? oldValue = "Old",
            string? newValue = "New",
            int? userId = 1,
            DateTimeOffset? createdDate = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var log = new ItemLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = "Update",
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedBy = userId,
                CreatedDate = createdDate ?? DateTimeOffset.UtcNow
            };
            await ctx.ItemLog.AddAsync(log);
            await ctx.SaveChangesAsync();
            return log.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Row()
        {
            await ClearAsync();
            var id = await SeedLogAsync(propertyName: "Color", oldValue: "Red", newValue: "Blue");

            var result = await CreateRepo().GetByIdAsync((int)id);

            result.Should().NotBeNull();
            result!.PropertyName.Should().Be("Color");
            result.OldValue.Should().Be("Red");
            result.NewValue.Should().Be("Blue");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetByIdAsync(999999);

            result.Should().BeNull();
        }

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_All_With_TotalCount()
        {
            await ClearAsync();
            await SeedLogAsync(propertyName: "A");
            await SeedLogAsync(propertyName: "B");

            var (items, total) = await CreateRepo().GetAllAsync(new ItemLogFilter());

            items.Should().HaveCount(2);
            total.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_EntityId()
        {
            await ClearAsync();
            await SeedLogAsync(entityId: 10);
            await SeedLogAsync(entityId: 20);

            var (items, total) = await CreateRepo().GetAllAsync(new ItemLogFilter { EntityId = 10 });

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].EntityId.Should().Be(10);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_UserId()
        {
            await ClearAsync();
            await SeedLogAsync(userId: 100);
            await SeedLogAsync(userId: 200);

            var (items, _) = await CreateRepo().GetAllAsync(new ItemLogFilter { UserId = 100 });

            items.Should().HaveCount(1);
            items[0].CreatedBy.Should().Be(100);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_DateRange()
        {
            await ClearAsync();
            await SeedLogAsync(createdDate: DateTimeOffset.UtcNow.AddDays(-5), propertyName: "Old");
            await SeedLogAsync(createdDate: DateTimeOffset.UtcNow.AddHours(-1), propertyName: "Recent");

            var from = DateTime.UtcNow.AddDays(-1);
            var (items, _) = await CreateRepo().GetAllAsync(new ItemLogFilter { From = from });

            items.Should().HaveCount(1);
            items[0].PropertyName.Should().Be("Recent");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedLogAsync(propertyName: "Color", newValue: "Red");
            await SeedLogAsync(propertyName: "Size", newValue: "Large");

            var (items, _) = await CreateRepo().GetAllAsync(new ItemLogFilter { Search = "Color" });

            items.Should().HaveCount(1);
            items[0].PropertyName.Should().Be("Color");
        }

        [Fact]
        public async Task GetAllAsync_Should_Paginate_When_Page_And_Size_Set()
        {
            await ClearAsync();
            for (int i = 0; i < 5; i++)
                await SeedLogAsync(propertyName: $"P{i}");

            var (items, total) = await CreateRepo().GetAllAsync(new ItemLogFilter { Page = 1, Size = 3 });

            items.Should().HaveCount(3);
            total.Should().Be(5);
        }

        [Fact]
        public async Task GetAllAsync_Should_Order_By_CreatedDate_Descending()
        {
            await ClearAsync();
            await SeedLogAsync(createdDate: DateTimeOffset.UtcNow.AddDays(-2), propertyName: "Old");
            await SeedLogAsync(createdDate: DateTimeOffset.UtcNow.AddDays(-1), propertyName: "Mid");
            await SeedLogAsync(createdDate: DateTimeOffset.UtcNow, propertyName: "New");

            var (items, _) = await CreateRepo().GetAllAsync(new ItemLogFilter());

            items[0].PropertyName.Should().Be("New");
            items[2].PropertyName.Should().Be("Old");
        }

        // --- GetForEntityAsync ---

        [Fact]
        public async Task GetForEntityAsync_Should_Filter_By_EntityId()
        {
            await ClearAsync();
            await SeedLogAsync(entityId: 42, propertyName: "Target");
            await SeedLogAsync(entityId: 99, propertyName: "Other");

            var (items, _) = await CreateRepo().GetForEntityAsync("Item", 42, null, null);

            items.Should().HaveCount(1);
            items[0].EntityId.Should().Be(42);
        }
    }
}
