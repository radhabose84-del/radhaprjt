using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Reports.StockLedger;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StockLedger
{
    /// <summary>
    /// Integration tests for StockLedgerReportRepository (Dapper, read-only report).
    /// StockLedger is not a BaseEntity -- rows are seeded via raw SQL.
    /// The report joins to MiscMaster for StatusName, so MiscTypeMaster + MiscMaster are prerequisites.
    /// Cross-module lookups (Unit, Item, Warehouse, Bin, PackType, Lot) are mocked.
    /// InternalsVisibleTo is set in SalesManagement.Infrastructure.csproj.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class StockLedgerReportRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StockLedgerReportRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private StockLedgerReportRepository CreateRepo(
            Mock<IIPAddressService> ipService = null)
        {
            ipService ??= BuildDefaultIpService();

            return new StockLedgerReportRepository(
                new SqlConnection(_fixture.ConnectionString),
                BuildDefaultUnitLookup().Object,
                BuildDefaultItemLookup().Object,
                BuildDefaultWarehouseLookup().Object,
                BuildDefaultBinLookup().Object,
                BuildDefaultPackTypeLookup().Object,
                BuildDefaultLotLookup().Object,
                ipService.Object);
        }

        private static Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUnitId()).Returns(1);
            mock.Setup(x => x.GetCompanyId()).Returns(1);
            mock.Setup(x => x.GetUserId()).Returns(1);
            mock.Setup(x => x.GetUserName()).Returns("test-user");
            mock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            return mock;
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup()
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit 1" }
                });
            return mock;
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemName = "Item " + id }).ToList());
            return mock;
        }

        private static Mock<IWarehouseLookup> BuildDefaultWarehouseLookup()
        {
            var mock = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            mock.Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>
                {
                    new WarehouseLookupDto { Id = 1, WarehouseName = "WH 1" }
                });
            return mock;
        }

        private static Mock<IBinLookup> BuildDefaultBinLookup()
        {
            var mock = new Mock<IBinLookup>(MockBehavior.Loose);
            mock.Setup(b => b.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>
                {
                    new BinLookupDto { Id = 1, BinName = "Bin 1" }
                });
            return mock;
        }

        private static Mock<IPackTypeLookup> BuildDefaultPackTypeLookup()
        {
            var mock = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PackTypeLookupDto>)ids.Select(id =>
                        new PackTypeLookupDto { Id = id, PackTypeName = "PackType " + id }).ToList());
            return mock;
        }

        private static Mock<ILotMasterLookup> BuildDefaultLotLookup()
        {
            var mock = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            mock.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<LotMasterLookupDto>)ids.Select(id =>
                        new LotMasterLookupDto { Id = id, LotCode = "LOT" + id }).ToList());
            return mock;
        }

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(string code = "StockStatus")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Stock Status",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        private async Task<int> EnsureMiscAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task SeedStockLedgerRowAsync(
            int statusId,
            int unitId = 1,
            int itemId = 100,
            int lotId = 10,
            int packNo = 1,
            int packTypeId = 1,
            int warehouseId = 1,
            int binId = 1,
            int totalQty = 100,
            decimal totalValue = 500m,
            string docType = "PROD",
            int docNo = 1,
            DateOnly? docDate = null)
        {
            var date = docDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                INSERT INTO Sales.StockLedger
                    (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId,
                     PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId)
                VALUES
                    (@UnitId, @DocType, @DocNo, 0, @DocDate, @ItemId, @LotId,
                     @PackNo, @PackTypeId, @WarehouseId, @BinId, @TotalQty, @TotalValue, @StatusId)",
                new
                {
                    UnitId = unitId,
                    DocType = docType,
                    DocNo = docNo,
                    DocDate = date.ToDateTime(TimeOnly.MinValue),
                    ItemId = itemId,
                    LotId = lotId,
                    PackNo = packNo,
                    PackTypeId = packTypeId,
                    WarehouseId = warehouseId,
                    BinId = binId,
                    TotalQty = totalQty,
                    TotalValue = totalValue,
                    StatusId = statusId
                });
        }

        // ---------------------------------------------------------------------------
        // GetReportAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetReportAsync_Should_Return_Seeded_Records()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");
            var mtId = await EnsureMiscTypeAsync();
            var statusId = await EnsureMiscAsync(mtId, "Packed");

            await SeedStockLedgerRowAsync(statusId, itemId: 100, packNo: 1);
            await SeedStockLedgerRowAsync(statusId, itemId: 100, packNo: 2);

            var (data, total) = await CreateRepo().GetReportAsync(
                1, 10, null, null, null, null, null, null, null, null, DateTime.UtcNow.Year);

            total.Should().BeGreaterOrEqualTo(2);
            data.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetReportAsync_Should_Return_Empty_When_No_Data()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");

            var (data, total) = await CreateRepo().GetReportAsync(
                1, 10, null, null, null, null, null, null, null, null, 1900);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetReportAsync_Should_Filter_By_ItemId()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");
            var mtId = await EnsureMiscTypeAsync();
            var statusId = await EnsureMiscAsync(mtId, "Packed");

            await SeedStockLedgerRowAsync(statusId, itemId: 200, packNo: 1);
            await SeedStockLedgerRowAsync(statusId, itemId: 300, packNo: 2);

            var (data, total) = await CreateRepo().GetReportAsync(
                1, 10, itemId: 200, null, null, null, null, null, null, null, DateTime.UtcNow.Year);

            total.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].ItemId.Should().Be(200);
        }

        [Fact]
        public async Task GetReportAsync_Should_Populate_Lookup_Names()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");
            var mtId = await EnsureMiscTypeAsync();
            var statusId = await EnsureMiscAsync(mtId, "Packed");

            await SeedStockLedgerRowAsync(statusId, itemId: 100, warehouseId: 1, binId: 1);

            var (data, _) = await CreateRepo().GetReportAsync(
                1, 10, null, null, null, null, null, null, null, null, DateTime.UtcNow.Year);

            data.Should().NotBeEmpty();
            data[0].ItemName.Should().Be("Item 100");
            data[0].UnitName.Should().Be("Unit 1");
            data[0].WarehouseName.Should().Be("WH 1");
            data[0].BinName.Should().Be("Bin 1");
        }

        [Fact]
        public async Task GetReportAsync_Should_Support_Pagination()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");
            var mtId = await EnsureMiscTypeAsync();
            var statusId = await EnsureMiscAsync(mtId, "Packed");

            for (int i = 1; i <= 5; i++)
                await SeedStockLedgerRowAsync(statusId, itemId: 100, packNo: i);

            var (page1, total) = await CreateRepo().GetReportAsync(
                1, 3, null, null, null, null, null, null, null, null, DateTime.UtcNow.Year);
            var (page2, _) = await CreateRepo().GetReportAsync(
                2, 3, null, null, null, null, null, null, null, null, DateTime.UtcNow.Year);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
        }

        // ---------------------------------------------------------------------------
        // GetByPackRangeAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByPackRangeAsync_Should_Return_Packs_In_Range()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");
            var mtId = await EnsureMiscTypeAsync();
            var statusId = await EnsureMiscAsync(mtId, "Packed");

            for (int i = 1; i <= 5; i++)
                await SeedStockLedgerRowAsync(statusId, itemId: 100, packNo: i, packTypeId: 1);

            var result = await CreateRepo().GetByPackRangeAsync(
                100, 1, 2, 4, DateTime.UtcNow.Year);

            result.Should().HaveCount(3);
            result.Select(r => r.PackNo).Should().Contain(new[] { 2, 3, 4 });
        }

        [Fact]
        public async Task GetByPackRangeAsync_Should_Return_Empty_When_No_Match()
        {
            await _fixture.ClearTablesAsync("Sales.StockLedger");

            var result = await CreateRepo().GetByPackRangeAsync(
                999, 999, 1, 10, 1900);

            result.Should().BeEmpty();
        }
    }
}
