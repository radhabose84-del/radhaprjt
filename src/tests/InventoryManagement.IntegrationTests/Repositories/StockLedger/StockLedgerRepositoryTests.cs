using InventoryManagement.Domain.Entities.Stock;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Stock;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Stock
{
    [Collection("DatabaseCollection")]
    public sealed class StockLedgerRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockLedgerRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StockLedgerRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTablesAsync(ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        // --- INSERT STOCK LEDGER ---

        [Fact]
        public async Task InsertStockLedgerDataAsync_Should_Persist_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entries = new List<InventoryManagement.Domain.Entities.Stock.StockLedger>
            {
                new()
                {
                    UnitId = 1, DocType = "GR", DocNo = 1, DocSlNo = 1,
                    DocDate = DateTime.UtcNow, ItemId = 1, UomId = 1,
                    WarehouseId = 1, StorageTypeId = 1, TargetId = 1,
                    ReceivedQty = 100m, ReceivedValue = 5000m
                }
            };

            await CreateRepo(ctx).InsertStockLedgerDataAsync(entries);
            ctx.ChangeTracker.Clear();

            var count = await ctx.StockLedger.CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task InsertStockLedgerDataAsync_EmptyList_Should_Not_Throw()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var act = () => CreateRepo(ctx).InsertStockLedgerDataAsync(new List<InventoryManagement.Domain.Entities.Stock.StockLedger>());

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task InsertStockLedgerDataAsync_NullList_Should_Not_Throw()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var act = () => CreateRepo(ctx).InsertStockLedgerDataAsync(null!);

            await act.Should().NotThrowAsync();
        }

        // --- INSERT SUBSTORE STOCK LEDGER ---

        [Fact]
        public async Task InsertSubStoreStockLedgerDataAsync_Should_Persist_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entries = new List<SubStoreStockLedger>
            {
                new()
                {
                    UnitId = 1, DocType = "MRS", DocNo = 1, DocSlNo = 1,
                    DocDate = DateTime.UtcNow, DepartmentId = 1, ItemId = 1,
                    UomId = 1, WarehouseId = 1,
                    ReceivedQty = 50m, ReceivedValue = 2500m
                }
            };

            await CreateRepo(ctx).InsertSubStoreStockLedgerDataAsync(entries);
            ctx.ChangeTracker.Clear();

            var count = await ctx.SubStoreStockLedger.CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task InsertSubStoreStockLedgerDataAsync_EmptyList_Should_Not_Throw()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var act = () => CreateRepo(ctx).InsertSubStoreStockLedgerDataAsync(new List<SubStoreStockLedger>());

            await act.Should().NotThrowAsync();
        }
    }
}
