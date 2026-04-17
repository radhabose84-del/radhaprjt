using Contracts.Dtos.Stock;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class StockLedgerLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockLedgerLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private StockLedgerLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private static StockLedgerDto BuildLedger(int docNo = 1, int itemId = 1) =>
            new()
            {
                UnitId = 1,
                DocType = "GRN",
                DocNo = docNo,
                DocSlNo = 1,
                DocDate = DateTime.UtcNow.Date,
                ItemId = itemId,
                UomId = 1,
                WarehouseId = 1,
                StorageTypeId = 1,
                TargetId = 1,
                ReceivedQty = 100m,
                ReceivedValue = 1000m,
                IssueQty = 0m,
                IssueValue = 0m
            };

        private static SubStoreStockLedgerDto BuildSubStoreLedger(int docNo = 1) =>
            new()
            {
                UnitId = 1, DocType = "GRN", DocNo = docNo, DocSlNo = 1,
                DocDate = DateTime.UtcNow.Date,
                DepartmentId = 1, ItemId = 1, UomId = 1,
                WarehouseId = 1, StorageTypeId = 1, TargetId = 1,
                ReceivedQty = 50m, ReceivedValue = 500m,
                IssueQty = 0m, IssueValue = 0m
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- InsertStockLedgerAsync ---

        [Fact]
        public async Task InsertStockLedgerAsync_Should_Return_True_And_Persist_Rows()
        {
            await ClearAsync();
            var rows = new List<StockLedgerDto> { BuildLedger(100), BuildLedger(101) };

            var result = await CreateRepo().InsertStockLedgerAsync(rows);

            result.Should().BeTrue();
            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.StockLedger.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task InsertStockLedgerAsync_Should_Return_True_And_NoOp_For_EmptyList()
        {
            await ClearAsync();

            var result = await CreateRepo().InsertStockLedgerAsync(new List<StockLedgerDto>());

            result.Should().BeTrue();
            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.StockLedger.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task InsertStockLedgerAsync_Should_Return_True_When_Input_Null()
        {
            await ClearAsync();

            var result = await CreateRepo().InsertStockLedgerAsync(null!);

            result.Should().BeTrue();
        }

        // --- InsertSubStoreStockLedgerAsync ---

        [Fact]
        public async Task InsertSubStoreStockLedgerAsync_Should_Persist_Rows()
        {
            await ClearAsync();
            var rows = new List<SubStoreStockLedgerDto> { BuildSubStoreLedger(200) };

            var result = await CreateRepo().InsertSubStoreStockLedgerAsync(rows);

            result.Should().BeTrue();
            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.SubStoreStockLedger.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task InsertSubStoreStockLedgerAsync_Should_NoOp_For_EmptyList()
        {
            await ClearAsync();

            var result = await CreateRepo().InsertSubStoreStockLedgerAsync(new List<SubStoreStockLedgerDto>());

            result.Should().BeTrue();
        }
    }
}
