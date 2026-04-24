using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.ProductionPack;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.ProductionPack
{
    [Collection("DatabaseCollection")]
    public sealed class ProductionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProductionQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProductionQueryRepository CreateRepo(
            Mock<IUnitLookup>? unit = null,
            Mock<IWarehouseLookup>? wh = null,
            Mock<IBinLookup>? bin = null,
            Mock<IItemLookup>? item = null,
            Mock<ISalesStockLedgerService>? stockLedger = null)
        {
            unit ??= new Mock<IUnitLookup>(MockBehavior.Loose);
            wh ??= new Mock<IWarehouseLookup>(MockBehavior.Loose);
            bin ??= new Mock<IBinLookup>(MockBehavior.Loose);
            item ??= new Mock<IItemLookup>(MockBehavior.Loose);
            stockLedger ??= new Mock<ISalesStockLedgerService>(MockBehavior.Loose);

            // sane defaults
            unit.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto>());
            wh.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<WarehouseLookupDto>)new List<WarehouseLookupDto>());
            bin.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());
            item.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            return new ProductionQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unit.Object, wh.Object, bin.Object, item.Object,
                _fixture.IpMock.Object, stockLedger.Object);
        }

        private async Task<int> SeedLotMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PP_LM_T")
                ?? new Domain.Entities.MiscTypeMaster { MiscTypeCode = "PP_LM_T", Description = "T", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            if (t.Id == 0) { ctx.MiscTypeMaster.Add(t); await ctx.SaveChangesAsync(); }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PP_LM_M")
                ?? new Domain.Entities.MiscMaster { MiscTypeId = t.Id, Code = "PP_LM_M", Description = "M", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            if (m.Id == 0) { ctx.MiscMaster.Add(m); await ctx.SaveChangesAsync(); }
            var lm = new Domain.Entities.LotMaster
            {
                LotCode = $"LOT_{Guid.NewGuid():N}".Substring(0, 20),
                BatchNumber = "B",
                LotTypeId = m.Id, StatusId = m.Id,
                ItemId = 1, UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.LotMaster.AddAsync(lm);
            await ctx.SaveChangesAsync();
            return lm.Id;
        }

        private async Task<int> SeedProductionAsync(string packNo, int lotId,
            int? startPackNo = null, int? endPackNo = null,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new Domain.Entities.ProductionPackEntry
            {
                PackNo = packNo,
                PackDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ProductionYear = DateTime.UtcNow.Year,
                UnitId = 1, WarehouseId = 1,
                ItemId = 1, LotId = lotId,
                StartPackNo = startPackNo, EndPackNo = endPackNo,
                TotalBags = 0, TotalNetWeight = 0m,
                IsActive = Status.Active, IsDeleted = deleted
            };
            await ctx.ProductionPackEntry.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await SeedProductionAsync("P1", lotId);

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await SeedProductionAsync("PDEL", lotId, deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            var id = await SeedProductionAsync("PID", lotId);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PackNo.Should().Be("PID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            var id = await SeedProductionAsync("PSD", lotId, deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            var id = await SeedProductionAsync("PNF", lotId);

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task LotExistsAsync_Should_Return_True_For_Seeded()
        {
            var lotId = await SeedLotMasterAsync();

            var result = await CreateRepo().LotExistsAsync(lotId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task LotExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().LotExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().PackTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task QualityStatusExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().QualityStatusExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UnitExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(l => l.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 1, UnitName = "U1" });

            var result = await CreateRepo(unit: unit).UnitExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Lookup_Empty()
        {
            var result = await CreateRepo().ItemExistsAsync(9999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Return_False_When_No_Overlap()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await SeedProductionAsync("POV1", lotId, startPackNo: 1, endPackNo: 10);

            var result = await CreateRepo().PackOverlapExistsAsync(lotId, 100, 200);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Return_True_For_Overlap()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await SeedProductionAsync("POV2", lotId, startPackNo: 1, endPackNo: 10);

            var result = await CreateRepo().PackOverlapExistsAsync(lotId, 5, 15);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            var id = await SeedProductionAsync("POV3", lotId, startPackNo: 1, endPackNo: 10);

            var result = await CreateRepo().PackOverlapExistsAsync(lotId, 5, 15, excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetLastEndPackNoAsync_Should_Delegate_To_StockLedgerLookup()
        {
            var stockLedger = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            stockLedger.Setup(s => s.GetLastPackNoByYearAsync(2025, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var result = await CreateRepo(stockLedger: stockLedger).GetLastEndPackNoAsync(2025);

            result.Should().Be(42);
        }
    }
}
