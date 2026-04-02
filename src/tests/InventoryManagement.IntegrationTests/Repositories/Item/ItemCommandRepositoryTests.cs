using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static ItemMaster BuildEntity(string code = "ITEM001", string name = "Test Item") =>
            new()
            {
                ItemCode = code,
                ItemName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantValue]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemVariantAttribute]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUsageTypeMapping]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemUOM]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSupplier]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemManufacture]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemSale]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemQuality]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemPurchase]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemInventory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemLogs]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync(@"
                UPDATE [Inventory].[ItemMaster] SET ParentItemId = NULL;
                DELETE FROM [Inventory].[ItemMaster];
            ");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("ITEM002", "Widget Alpha"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved.Should().NotBeNull();
            saved!.ItemCode.Should().Be("ITEM002");
            saved.ItemName.Should().Be("Widget Alpha");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- EXISTS BY CODE ---

        [Fact]
        public async Task ExistsByCodeForCreateAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepo(ctx).CreateAsync(BuildEntity("DUPCODE"));
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepo(ctx).ExistsByCodeForCreateAsync("DUPCODE");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeForCreateAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepo(ctx).ExistsByCodeForCreateAsync("NOCODE");

            exists.Should().BeFalse();
        }

        // --- GET TRACKING ---

        [Fact]
        public async Task GetTrackingAsync_Should_Return_Entity_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TRACK01"));
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepo(ctx).GetTrackingAsync(id);

            entity.Should().NotBeNull();
            entity!.ItemCode.Should().Be("TRACK01");
        }

        [Fact]
        public async Task GetTrackingAsync_Should_Return_Null_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepo(ctx).GetTrackingAsync(99999);

            entity.Should().BeNull();
        }

        // --- EXISTS BY NAME ---

        [Fact]
        public async Task ExistsByNameSmartForCreateAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepo(ctx).CreateAsync(BuildEntity("NM001", "Bolt M10"));
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepo(ctx).ExistsByNameSmartForCreateAsync("Bolt M10");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameSmartForCreateAsync_Should_Return_False_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepo(ctx).ExistsByNameSmartForCreateAsync("NonExistent");

            exists.Should().BeFalse();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("UPD01", "Old Name"));
            ctx.ChangeTracker.Clear();

            var entity = await CreateRepo(ctx).GetTrackingAsync(id);
            entity!.ItemName = "Updated Name";
            await CreateRepo(ctx).UpdateAsync(entity);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemMaster.FirstAsync(x => x.Id == id);
            saved.ItemName.Should().Be("Updated Name");
        }
    }
}
