using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class SalesStockLedgerLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        private const int ProductionYear = 2026;

        public SalesStockLedgerLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── SUT helpers ──────────────────────────────────────────────────────

        private SalesStockLedgerLookupRepository CreateRepo(
            Mock<IItemLookup> itemLookup = null,
            Mock<ILotMasterLookup> lotLookup = null,
            Mock<IPackTypeLookup> packTypeLookup = null,
            Mock<IUnitLookup> unitLookup = null)
        {
            itemLookup ??= BuildItemLookupMock();
            lotLookup ??= BuildLotLookupMock();
            packTypeLookup ??= BuildPackTypeLookupMock();
            unitLookup ??= BuildUnitLookupMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesStockLedgerLookupRepository(conn, itemLookup.Object, lotLookup.Object, packTypeLookup.Object, unitLookup.Object);
        }

        private static Mock<IItemLookup> BuildItemLookupMock(params (int id, string name)[] items)
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            var list = items.Select(i => new ItemLookupDto { Id = i.id, ItemName = i.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private static Mock<ILotMasterLookup> BuildLotLookupMock(params LotMasterLookupDto[] lots)
        {
            var mock = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lots.ToList());
            return mock;
        }

        private static Mock<IPackTypeLookup> BuildPackTypeLookupMock(params PackTypeLookupDto[] packTypes)
        {
            var mock = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(packTypes.ToList());
            return mock;
        }

        private static Mock<IUnitLookup> BuildUnitLookupMock()
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            return mock;
        }

        // ── Seed helpers ─────────────────────────────────────────────────────

        private async Task<int> SeedMiscTypeAsync(string code)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Sales.MiscTypeMaster
                    (MiscTypeCode, Description, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@Code, @Code, 1, 0, 1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');",
                new { Code = code });
        }

        private async Task<int> SeedMiscAsync(int miscTypeId, string code, string description)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Sales.MiscMaster
                    (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    (@TypeId, @Code, @Desc, 1, 1, 0, 1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');",
                new { TypeId = miscTypeId, Code = code, Desc = description });
        }

        private async Task InsertLedgerRowAsync(
            int unitId, string docType, int docNo, int packNo, int packTypeId, int itemId, int lotId, int statusId,
            decimal totalValue = 10m, int? year = null)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var docDate = new DateOnly(year ?? ProductionYear, 6, 1);
            await conn.ExecuteAsync(@"
                INSERT INTO Sales.StockLedger
                    (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId, PackNo,
                     PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId, TypeId)
                VALUES
                    (@UnitId, @DocType, @DocNo, 1, @DocDate, @ItemId, @LotId, @PackNo,
                     @PackTypeId, 1, 1, 100, @TotalValue, @StatusId, 1);",
                new
                {
                    UnitId = unitId, DocType = docType, DocNo = docNo,
                    DocDate = docDate, ItemId = itemId, LotId = lotId, PackNo = packNo,
                    PackTypeId = packTypeId, TotalValue = totalValue, StatusId = statusId
                });
        }

        private async Task<int> CountLedgerRowsAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Sales.StockLedger");
        }

        private async Task<int> GetStatusIdAsync(int packNo)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(
                "SELECT StatusId FROM Sales.StockLedger WHERE PackNo = @P",
                new { P = packNo });
        }

        private async Task ClearAsync()
        {
            // StockLedger tests seed rows in Sales.StockLedger (unique index on DocType+DocNo+PackNo)
            // and Sales.MiscTypeMaster (unique index on MiscTypeCode 'PackStatus'). Order:
            //   1. Wipe StockLedger first (FK StockLedger.StatusId → MiscMaster.Id).
            //   2. Call ClearAllMiscMasterDependentsAsync (drops MiscMaster + aggregate chains).
            //   3. Wipe MiscTypeMaster last (FK MiscMaster.MiscTypeId → MiscTypeMaster.Id now clear).
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.StockLedger;");

            await _fixture.ClearAllMiscMasterDependentsAsync();

            await conn.ExecuteAsync("DELETE FROM Sales.MiscTypeMaster;");
        }

        // ── InsertAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task InsertAsync_Returns_False_For_Null_Input()
        {
            var result = await CreateRepo().InsertAsync(null);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task InsertAsync_Returns_False_For_Empty_List()
        {
            var result = await CreateRepo().InsertAsync(new List<SalesStockLedgerDto>());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task InsertAsync_Persists_Rows()
        {
            await ClearAsync();

            var entries = new List<SalesStockLedgerDto>
            {
                new() { UnitId = 1, DocType = "DC", DocNo = 100, DetailDocNo = 1, DocDate = new DateTime(ProductionYear, 1, 1),
                        ItemId = 10, LotId = 1, PackNo = 1, PackTypeId = 1, WarehouseId = 1, BinId = 1,
                        TotalQty = 100, TotalValue = 95m, StatusId = 1, TypeId = 1 },
                new() { UnitId = 1, DocType = "DC", DocNo = 100, DetailDocNo = 2, DocDate = new DateTime(ProductionYear, 1, 1),
                        ItemId = 10, LotId = 1, PackNo = 2, PackTypeId = 1, WarehouseId = 1, BinId = 1,
                        TotalQty = 100, TotalValue = 95m, StatusId = 1, TypeId = 1 }
            };

            var result = await CreateRepo().InsertAsync(entries);

            result.Should().BeTrue();
            (await CountLedgerRowsAsync()).Should().Be(2);
        }

        // ── DeleteByDocAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task DeleteByDocAsync_Removes_Matching_Rows()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 5, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 5, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 6, packNo: 3, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);

            var result = await CreateRepo().DeleteByDocAsync("DC", 5, ProductionYear, 1);

            result.Should().BeTrue();
            (await CountLedgerRowsAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteByDocAsync_Returns_False_When_NoMatch()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 5, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);

            var result = await CreateRepo().DeleteByDocAsync("STO", 999, ProductionYear, 1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteByDocAsync_Respects_Year_And_UnitId()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 5, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 2, docType: "DC", docNo: 5, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 5, packNo: 3, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1, year: 2025);

            var result = await CreateRepo().DeleteByDocAsync("DC", 5, ProductionYear, 1);

            result.Should().BeTrue();
            // only the UnitId=1 / Year=2026 row got deleted
            (await CountLedgerRowsAsync()).Should().Be(2);
        }

        // ── UpdateStatusByPackRangeAsync ─────────────────────────────────────

        [Fact]
        public async Task UpdateStatusByPackRangeAsync_Updates_Rows_In_Range()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 3, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 4, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);

            var result = await CreateRepo().UpdateStatusByPackRangeAsync(
                docType: "DC", startPackNo: 2, endPackNo: 3,
                currentStatusId: 1, newStatusId: 99,
                productionYear: ProductionYear, unitId: 1);

            result.Should().BeTrue();
            (await GetStatusIdAsync(1)).Should().Be(1);
            (await GetStatusIdAsync(2)).Should().Be(99);
            (await GetStatusIdAsync(3)).Should().Be(99);
            (await GetStatusIdAsync(4)).Should().Be(1);
        }

        [Fact]
        public async Task UpdateStatusByPackRangeAsync_Ignores_Rows_With_Different_CurrentStatus()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: 5);

            var result = await CreateRepo().UpdateStatusByPackRangeAsync(
                docType: "DC", startPackNo: 1, endPackNo: 2,
                currentStatusId: 1, newStatusId: 99,
                productionYear: ProductionYear, unitId: 1);

            result.Should().BeTrue();
            (await GetStatusIdAsync(1)).Should().Be(99);
            (await GetStatusIdAsync(2)).Should().Be(5);  // unchanged
        }

        [Fact]
        public async Task UpdateStatusByPackRangeAsync_Returns_False_When_NoMatch()
        {
            await ClearAsync();

            var result = await CreateRepo().UpdateStatusByPackRangeAsync(
                docType: "DC", startPackNo: 1, endPackNo: 10,
                currentStatusId: 1, newStatusId: 2,
                productionYear: ProductionYear, unitId: 1);

            result.Should().BeFalse();
        }

        // ── GetPackedPackNosAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetPackedPackNosAsync_Returns_PackNos_Where_Status_Is_Packed()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("PackStatus");
            var packedId = await SeedMiscAsync(typeId, "Packed", "Packed");
            var shippedId = await SeedMiscAsync(typeId, "Shipped", "Shipped");

            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: packedId);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: shippedId);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 3, packTypeId: 1, itemId: 10, lotId: 1, statusId: packedId);

            var result = await CreateRepo().GetPackedPackNosAsync(
                startPackNo: 1, endPackNo: 10,
                productionYear: ProductionYear, unitId: 1);

            result.Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task GetPackedPackNosAsync_Returns_Empty_When_NoPacked()
        {
            await ClearAsync();

            var result = await CreateRepo().GetPackedPackNosAsync(
                startPackNo: 1, endPackNo: 10,
                productionYear: ProductionYear, unitId: 1);

            result.Should().BeEmpty();
        }

        // ── GetStockItemsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetStockItemsAsync_Groups_Packs_By_Item_With_Item_Name()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("PackStatus");
            var packedId = await SeedMiscAsync(typeId, "Packed", "Packed");

            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 1, itemId: 10, lotId: 1, statusId: packedId, totalValue: 95m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 1, itemId: 10, lotId: 1, statusId: packedId, totalValue: 95m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 3, packTypeId: 1, itemId: 20, lotId: 1, statusId: packedId, totalValue: 50m);

            var result = await CreateRepo(
                itemLookup: BuildItemLookupMock((10, "Item A"), (20, "Item B")))
                .GetStockItemsAsync(productionYear: ProductionYear, unitId: 1);

            result.Should().HaveCount(2);
            var itemA = result.First(x => x.ItemId == 10);
            itemA.TotalBags.Should().Be(2);
            itemA.TotalNetWeight.Should().Be(190m);
            itemA.ItemName.Should().Be("Item A");
            result.First(x => x.ItemId == 20).ItemName.Should().Be("Item B");
        }

        [Fact]
        public async Task GetStockItemsAsync_Returns_Empty_When_NoPacked()
        {
            await ClearAsync();

            var result = await CreateRepo().GetStockItemsAsync(productionYear: ProductionYear, unitId: 1);

            result.Should().BeEmpty();
        }

        // ── GetLastPackNoByYearAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetLastPackNoByYearAsync_Returns_Max_PackNo()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 10, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 25, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 18, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);

            var result = await CreateRepo().GetLastPackNoByYearAsync(productionYear: ProductionYear, unitId: 1);

            result.Should().Be(25);
        }

        [Fact]
        public async Task GetLastPackNoByYearAsync_Returns_Zero_When_NoRows()
        {
            await ClearAsync();

            var result = await CreateRepo().GetLastPackNoByYearAsync(productionYear: ProductionYear, unitId: 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task GetLastPackNoByYearAsync_Filters_By_Year_And_UnitId()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 50, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1, year: 2025);
            await InsertLedgerRowAsync(unitId: 2, docType: "DC", docNo: 1, packNo: 75, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 10, packTypeId: 1, itemId: 10, lotId: 1, statusId: 1);

            var result = await CreateRepo().GetLastPackNoByYearAsync(productionYear: ProductionYear, unitId: 1);

            result.Should().Be(10);
        }

        // ── GetLotIdByPackRangeAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetLotIdByPackRangeAsync_Returns_First_NonZero_LotId()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 5, packTypeId: 1, itemId: 10, lotId: 0, statusId: 1);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 6, packTypeId: 1, itemId: 10, lotId: 77, statusId: 1);

            var result = await CreateRepo().GetLotIdByPackRangeAsync(
                startPackNo: 5, endPackNo: 10, productionYear: ProductionYear, unitId: 1);

            result.Should().Be(77);
        }

        // ── GetPackSourceInfoAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetPackSourceInfoAsync_Returns_Aggregated_Source()
        {
            await ClearAsync();
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 1, itemId: 10, lotId: 5, statusId: 1, totalValue: 100m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 1, itemId: 10, lotId: 5, statusId: 1, totalValue: 100m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 3, packTypeId: 1, itemId: 10, lotId: 5, statusId: 1, totalValue: 100m);

            var result = await CreateRepo().GetPackSourceInfoAsync(
                startPackNo: 1, endPackNo: 3, productionYear: ProductionYear, unitId: 1);

            result.Should().NotBeNull();
            result!.LotId.Should().Be(5);
            result.OldTotalBags.Should().Be(3);
            result.OldNetWeight.Should().Be(300m);
            result.OldNetWeightPerPack.Should().Be(100m);
        }

        [Fact]
        public async Task GetPackSourceInfoAsync_Returns_Null_When_NoMatch()
        {
            await ClearAsync();

            var result = await CreateRepo().GetPackSourceInfoAsync(
                startPackNo: 1, endPackNo: 10, productionYear: ProductionYear, unitId: 1);

            result.Should().BeNull();
        }

        // ── GetPacksByItemAndLotAsync ────────────────────────────────────────

        [Fact]
        public async Task GetPacksByItemAndLotAsync_NullLotId_Groups_By_Lot_And_PackType()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("PackStatus");
            var packedId = await SeedMiscAsync(typeId, "Packed", "Packed");

            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 100, itemId: 10, lotId: 5, statusId: packedId, totalValue: 95m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 100, itemId: 10, lotId: 5, statusId: packedId, totalValue: 95m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 3, packTypeId: 200, itemId: 10, lotId: 5, statusId: packedId, totalValue: 48m);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 4, packTypeId: 100, itemId: 10, lotId: 6, statusId: packedId, totalValue: 95m);

            var result = await CreateRepo(
                lotLookup: BuildLotLookupMock(
                    new LotMasterLookupDto { Id = 5, LotCode = "LOT005", BatchNumber = "B005" },
                    new LotMasterLookupDto { Id = 6, LotCode = "LOT006", BatchNumber = "B006" }),
                packTypeLookup: BuildPackTypeLookupMock(
                    new PackTypeLookupDto { Id = 100, PackTypeCode = "PT100", PackTypeName = "Big Bag" },
                    new PackTypeLookupDto { Id = 200, PackTypeCode = "PT200", PackTypeName = "Small Bag" }))
                .GetPacksByItemAndLotAsync(itemId: 10, lotId: null, productionYear: ProductionYear, unitId: 1);

            result.Should().HaveCount(3);
            var lot5Big = result.First(x => x.LotId == 5 && x.PackTypeId == 100);
            lot5Big.TotalBags.Should().Be(2);
            lot5Big.NetWeight.Should().Be(190m);
            lot5Big.LotCode.Should().Be("LOT005");
            lot5Big.PackTypeName.Should().Be("Big Bag");
        }

        [Fact]
        public async Task GetPacksByItemAndLotAsync_SpecificLotId_Filters_To_That_Lot()
        {
            await ClearAsync();
            var typeId = await SeedMiscTypeAsync("PackStatus");
            var packedId = await SeedMiscAsync(typeId, "Packed", "Packed");

            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 1, packTypeId: 100, itemId: 10, lotId: 5, statusId: packedId);
            await InsertLedgerRowAsync(unitId: 1, docType: "DC", docNo: 1, packNo: 2, packTypeId: 100, itemId: 10, lotId: 6, statusId: packedId);

            var result = await CreateRepo(
                lotLookup: BuildLotLookupMock(new LotMasterLookupDto { Id = 5, LotCode = "LOT005", BatchNumber = "B005" }),
                packTypeLookup: BuildPackTypeLookupMock(new PackTypeLookupDto { Id = 100, PackTypeCode = "PT100", PackTypeName = "Big Bag" }))
                .GetPacksByItemAndLotAsync(itemId: 10, lotId: 5, productionYear: ProductionYear, unitId: 1);

            result.Should().ContainSingle();
            result[0].LotId.Should().Be(5);
            result[0].LotCode.Should().Be("LOT005");
        }

        [Fact]
        public async Task GetPacksByItemAndLotAsync_Returns_Empty_When_NoMatches()
        {
            await ClearAsync();

            var result = await CreateRepo()
                .GetPacksByItemAndLotAsync(itemId: 10, lotId: null, productionYear: ProductionYear, unitId: 1);

            result.Should().BeEmpty();
        }
    }
}
