using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Production;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ItemQueryRepository CreateRepo()
        {
            IDbConnection conn = new SqlConnection(_fixture.ConnectionString);

            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            var countLookup = new Mock<ICountMasterLookup>(MockBehavior.Loose);

            var dataAccessFilter = new Mock<IDataAccessFilter>(MockBehavior.Loose);
            dataAccessFilter
                .Setup(f => f.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(DataAccessContext.Unrestricted);

            var ctx = _fixture.CreateFreshDbContext();

            return new ItemQueryRepository(
                conn, ctx, _fixture.IpMock.Object,
                unitLookup.Object, countLookup.Object, dataAccessFilter.Object);
        }

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
            await ctx.Database.ExecuteSqlRawAsync("UPDATE [Inventory].[ItemMaster] SET ParentItemId = NULL; DELETE FROM [Inventory].[ItemMaster]");
        }

        private async Task<int> SeedItemMasterAsync(string code = "QRY001", string name = "Query Test Item")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var item = new ItemMaster
            {
                ItemCode = code, ItemName = name,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await new ItemCommandRepository(ctx, _fixture.IpMock.Object).CreateAsync(item);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Items()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedItemMasterAsync("QRY001", "Alpha Widget");
            await SeedItemMasterAsync("QRY002", "Beta Widget");

            var repo = CreateRepo();
            var (items, total) = await repo.GetAllAsync(1, 10, null, false, null, null);

            total.Should().Be(2);
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Search()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedItemMasterAsync("QRY003", "Alpha Widget");
            await SeedItemMasterAsync("QRY004", "Beta Gear");

            var repo = CreateRepo();
            var (items, total) = await repo.GetAllAsync(1, 10, "Alpha", false, null, null);

            total.Should().Be(1);
            items[0].ItemName.Should().Contain("Alpha");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await SeedItemMasterAsync("QRY005", "Deleted Item");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.ItemMaster.FirstAsync(x => x.Id == id);
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx2.SaveChangesAsync();

            var repo = CreateRepo();
            var (items, total) = await repo.GetAllAsync(1, 10, null, false, null, null);

            total.Should().Be(0);
        }

        [Fact(Skip = "Requires stored procedure dbo.Sp_GetItemCode which is not created by EnsureCreatedAsync")]
        public async Task GetLatestItemCode_Should_Return_Null_When_NoItems()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepo();
            var code = await repo.GetLatestItemCode(999, 999);

            // May return null or a generated code depending on implementation
            // Just verify it doesn't throw
        }

        [Fact]
        public async Task GetCandidateItemNamesAsync_Should_Return_Matching_Names()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedItemMasterAsync("QRY010", "Steel Bolt M8");
            await SeedItemMasterAsync("QRY011", "Steel Nut M8");
            await SeedItemMasterAsync("QRY012", "Copper Wire");

            var repo = CreateRepo();
            var names = await repo.GetCandidateItemNamesAsync("steel");

            names.Should().HaveCount(2);
        }
    }
}
