using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemLogQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemLogQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemLogQueryRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemLogs]");
        }

        private async Task<long> SeedLogAsync(ApplicationDbContext ctx,
            string entity = "ItemMaster", int entityId = 1,
            string property = "ItemName", string? oldVal = "Old", string? newVal = "New")
        {
            var log = new ItemLog
            {
                EntityName = entity,
                EntityId = entityId,
                Action = "Update",
                PropertyName = property,
                OldValue = oldVal,
                NewValue = newVal,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };
            ctx.ItemLog.Add(log);
            await ctx.SaveChangesAsync();
            return log.Id;
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Log()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var logId = await SeedLogAsync(ctx, property: "Weight", oldVal: "1", newVal: "2");
            ctx.ChangeTracker.Clear();

            var dto = await CreateRepo(ctx).GetByIdAsync((int)logId);

            dto.Should().NotBeNull();
            dto!.PropertyName.Should().Be("Weight");
            dto.OldValue.Should().Be("1");
            dto.NewValue.Should().Be("2");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var dto = await CreateRepo(ctx).GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedLogAsync(ctx, entityId: 42);
            await SeedLogAsync(ctx, entityId: 42, property: "Description");
            ctx.ChangeTracker.Clear();

            var filter = new ItemLogFilter { EntityId = 42, Page = 1, Size = 10 };
            var (items, total) = await CreateRepo(ctx).GetAllAsync(filter);

            total.Should().Be(2);
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_EntityId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedLogAsync(ctx, entityId: 10);
            await SeedLogAsync(ctx, entityId: 20);
            ctx.ChangeTracker.Clear();

            var filter = new ItemLogFilter { EntityId = 10 };
            var (items, total) = await CreateRepo(ctx).GetAllAsync(filter);

            total.Should().Be(1);
            items[0].EntityId.Should().Be(10);
        }

        [Fact]
        public async Task GetForEntityAsync_Should_Return_Matching_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedLogAsync(ctx, entity: "ItemMaster", entityId: 5);
            ctx.ChangeTracker.Clear();

            var (items, total) = await CreateRepo(ctx).GetForEntityAsync("ItemMaster", 5, 1, 10);

            total.Should().Be(1);
            items[0].EntityId.Should().Be(5);
        }
    }
}
