using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Dtos.Stock;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
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

        // ── Mock factories ──────────────────────────────────────────────────

        private static Mock<IUnitLookup> BuildUnitLookup(int unitId = 1, string unitName = "Test Unit")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = unitId, UnitName = unitName });
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = unitId, UnitName = unitName }
                });
            return mock;
        }

        private static Mock<IWarehouseLookup> BuildWarehouseLookup(int whId = 1, string whName = "Test Warehouse")
        {
            var mock = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>
                {
                    new() { Id = whId, WarehouseName = whName }
                });
            return mock;
        }

        private static Mock<IBinLookup> BuildBinLookup(int binId = 1, string binName = "Test Bin")
        {
            var mock = new Mock<IBinLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>
                {
                    new() { Id = binId, BinName = binName }
                });
            mock.Setup(x => x.GetByWarehouseIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>
                {
                    new() { Id = binId, WarehouseId = 1, BinName = binName }
                });
            return mock;
        }

        private static Mock<IItemLookup> BuildItemLookup(int itemId = 1, string itemName = "Test Item")
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new() { Id = itemId, ItemName = itemName }
                });
            return mock;
        }

        private static Mock<IIPAddressService> BuildIpService(int? unitId = 1)
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUnitId()).Returns(unitId);
            return mock;
        }

        private static Mock<ISalesStockLedgerService> BuildStockLedger()
        {
            var mock = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            mock.Setup(x => x.GetLastPackNoByYearAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);
            return mock;
        }

        private ProductionQueryRepository CreateQueryRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IBinLookup>? binLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<IIPAddressService>? ipService = null,
            Mock<ISalesStockLedgerService>? stockLedger = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ProductionQueryRepository(
                conn,
                (unitLookup ?? BuildUnitLookup()).Object,
                (warehouseLookup ?? BuildWarehouseLookup()).Object,
                (binLookup ?? BuildBinLookup()).Object,
                (itemLookup ?? BuildItemLookup()).Object,
                (ipService ?? BuildIpService()).Object,
                (stockLedger ?? BuildStockLedger()).Object);
        }

        // ── Command repo for seeding ────────────────────────────────────────

        private static Mock<ISalesMiscMasterLookup> BuildMiscLookup()
        {
            var mock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1 });
            return mock;
        }

        private static Mock<IDocumentSequenceLookup> BuildDocSeq()
        {
            var mock = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            mock.Setup(x => x.IncrementDocNoAsync(
                    It.IsAny<int>(),
                    It.IsAny<System.Data.IDbConnection>(),
                    It.IsAny<System.Data.IDbTransaction>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        private static ProductionCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
        {
            var miscLookup = BuildMiscLookup();
            var stockLedger = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            stockLedger.Setup(x => x.InsertAsync(It.IsAny<List<SalesStockLedgerDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockLedger.Setup(x => x.DeleteByDocAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            return new ProductionCommandRepository(ctx, miscLookup.Object, stockLedger.Object, BuildDocSeq().Object);
        }

        // ── Prerequisites ───────────────────────────────────────────────────

        private async Task<(int lotId, int packTypeId, int qualityStatusId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            var miscType = new ProductionManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "QSTAT",
                Description = "Quality Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscRepo = new MiscMasterCommandRepository(ctx);

            var qualityStatusId = await miscRepo.CreateAsync(new ProductionManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "QSPK",
                Description = "Packed Quality",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var packTypeRepo = new PackTypeCommandRepository(ctx);
            var packTypeId = await packTypeRepo.CreateAsync(new Domain.Entities.PackType
            {
                PackTypeCode = "CONE",
                PackTypeName = "Cone Pack",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var lotTypeId = await miscRepo.CreateAsync(new ProductionManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LTYP",
                Description = "Lot Type",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var lotStatusId = await miscRepo.CreateAsync(new ProductionManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LSTS",
                Description = "Lot Status",
                SortOrder = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var lotRepo = new LotMasterCommandRepository(ctx);
            var lotId = await lotRepo.CreateAsync(new Domain.Entities.LotMaster
            {
                LotCode = "LOT001",
                BatchNumber = "BATCH001",
                LotTypeId = lotTypeId,
                ItemId = 1,
                UnitId = 1,
                StartDate = new DateOnly(2026, 1, 1),
                StatusId = lotStatusId,
                TotalProducedQty = 500m,
                AvailableQty = 500m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            return (lotId, packTypeId, qualityStatusId);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[ProductionPackDetail]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[ProductionPackHeader]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[LotMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[PackType]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private async Task<int> SeedHeaderAsync(
            ApplicationDbContext ctx,
            int lotId, int packTypeId, int qualityStatusId,
            string packNo = "PK-001")
        {
            var header = new ProductionPackHeader
            {
                PackNo = packNo,
                PackDate = new DateOnly(2026, 3, 1),
                ProductionYear = 2026,
                UnitId = 1,
                WarehouseId = 1,
                TotalBags = 5,
                TotalNetWeight = 125.0m,
                ProductionKgs = 130.0m,
                LooseConeKgs = 5.0m,
                Remarks = "Test pack",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ProductionPackDetails = new List<ProductionPackDetail>
                {
                    new()
                    {
                        ItemSno = 1,
                        LotId = lotId,
                        ItemId = 1,
                        PackTypeId = packTypeId,
                        NetWeightPerPack = 25.0m,
                        StartPackNo = 1,
                        EndPackNo = 5,
                        NoOfBags = 5,
                        TotalBags = 5,
                        TotalNetWeight = 125.0m,
                        BinId = 1,
                        QualityStatusId = qualityStatusId,
                        LineRemarks = "Detail 1"
                    }
                }
            };

            return await CreateCommandRepo(ctx).CreateAsync(header, typeId: 1);
        }

        // ── GET ALL ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_UnitName()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            var unitMock = BuildUnitLookup(1, "Acme Unit");
            var (items, _) = await CreateQueryRepo(unitLookup: unitMock).GetAllAsync(1, 10, null);

            items[0].UnitName.Should().Be("Acme Unit");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_WarehouseName()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            var whMock = BuildWarehouseLookup(1, "Main Warehouse");
            var (items, _) = await CreateQueryRepo(warehouseLookup: whMock).GetAllAsync(1, 10, null);

            items[0].WarehouseName.Should().Be("Main Warehouse");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            // Soft-delete directly in DB
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.ProductionPackHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId, "ALPHA-001");
            await SeedHeaderAsync(ctx, lotId, ptId, qsId, "BETA-002");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].PackNo.Should().Be("ALPHA-001");
        }

        // ── GET BY ID ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId, "PK-GBI");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.PackNo.Should().Be("PK-GBI");
            dto.ProductionPackDetails.Should().NotBeNull();
            dto.ProductionPackDetails!.Should().HaveCount(1);
            dto.ProductionPackDetails[0].NetWeightPerPack.Should().Be(25.0m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.ProductionPackHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // ── AUTOCOMPLETE ────────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId, "AUTOCOMP-01");

            var results = await CreateQueryRepo().AutocompleteAsync("AUTOCOMP", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].PackNo.Should().Be("AUTOCOMP-01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId, "INACT-01");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.ProductionPackHeader SET IsActive = 0 WHERE Id = {0}", id);

            var results = await CreateQueryRepo().AutocompleteAsync("INACT", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // ── NOT FOUND ───────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.ProductionPackHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── SAME-MODULE FK VALIDATION ───────────────────────────────────────

        [Fact]
        public async Task LotExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, _, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().LotExistsAsync(lotId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task LotExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().LotExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, ptId, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().PackTypeExistsAsync(ptId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().PackTypeExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task QualityStatusExistsAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, _, qsId) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().QualityStatusExistsAsync(qsId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task QualityStatusExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().QualityStatusExistsAsync(99999);

            result.Should().BeFalse();
        }

        // ── PACK OVERLAP ────────────────────────────────────────────────────

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Return_True_When_Overlapping()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId); // detail: StartPackNo=1, EndPackNo=5

            var result = await CreateQueryRepo().PackOverlapExistsAsync(lotId, 3, 7);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Return_False_When_No_Overlap()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            await SeedHeaderAsync(ctx, lotId, ptId, qsId); // detail: StartPackNo=1, EndPackNo=5

            var result = await CreateQueryRepo().PackOverlapExistsAsync(lotId, 6, 10);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackOverlapExistsAsync_Should_Exclude_Detail_By_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, ptId, qsId) = await SeedPrerequisitesAsync(ctx);
            var headerId = await SeedHeaderAsync(ctx, lotId, ptId, qsId);

            // Get the detail id to exclude
            var detail = await ctx.ProductionPackDetail
                .FirstAsync(d => d.ProductionPackHeaderId == headerId);

            var result = await CreateQueryRepo().PackOverlapExistsAsync(
                lotId, 1, 5, excludeDetailId: detail.Id);

            result.Should().BeFalse();
        }

        // ── CROSS-MODULE FK VALIDATION ──────────────────────────────────────

        [Fact]
        public async Task UnitExistsAsync_Should_Return_True_When_Lookup_Returns_Result()
        {
            var unitMock = BuildUnitLookup(1, "Unit1");
            var result = await CreateQueryRepo(unitLookup: unitMock).UnitExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UnitExistsAsync_Should_Return_False_When_Lookup_Returns_Null()
        {
            var unitMock = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UnitLookupDto?)null);

            var result = await CreateQueryRepo(unitLookup: unitMock).UnitExistsAsync(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task WarehouseExistsAsync_Should_Return_True_When_Found()
        {
            var whMock = BuildWarehouseLookup(1, "WH1");
            var result = await CreateQueryRepo(warehouseLookup: whMock).WarehouseExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task WarehouseExistsAsync_Should_Return_False_When_Empty()
        {
            var whMock = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            whMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>());

            var result = await CreateQueryRepo(warehouseLookup: whMock).WarehouseExistsAsync(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task BinExistsAsync_Should_Return_True_When_Found()
        {
            var binMock = BuildBinLookup(1, "Bin1");
            var result = await CreateQueryRepo(binLookup: binMock).BinExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_True_When_Found()
        {
            var itemMock = BuildItemLookup(1, "Item1");
            var result = await CreateQueryRepo(itemLookup: itemMock).ItemExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Empty()
        {
            var itemMock = new Mock<IItemLookup>(MockBehavior.Loose);
            itemMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());

            var result = await CreateQueryRepo(itemLookup: itemMock).ItemExistsAsync(99);

            result.Should().BeFalse();
        }

        // ── BIN BELONGS TO WAREHOUSE ────────────────────────────────────────

        [Fact]
        public async Task BinBelongsToWarehouseAsync_Should_Return_True_When_Match()
        {
            var binMock = BuildBinLookup(1, "Bin1");
            var result = await CreateQueryRepo(binLookup: binMock).BinBelongsToWarehouseAsync(1, 1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task BinBelongsToWarehouseAsync_Should_Return_False_When_No_Match()
        {
            var binMock = new Mock<IBinLookup>(MockBehavior.Loose);
            binMock.Setup(x => x.GetByWarehouseIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>());

            var result = await CreateQueryRepo(binLookup: binMock).BinBelongsToWarehouseAsync(1, 99);

            result.Should().BeFalse();
        }

        // ── GET LAST END PACK NO ────────────────────────────────────────────

        [Fact]
        public async Task GetLastEndPackNoAsync_Should_Delegate_To_StockLedger()
        {
            var stockMock = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            stockMock.Setup(x => x.GetLastPackNoByYearAsync(2026, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var result = await CreateQueryRepo(stockLedger: stockMock).GetLastEndPackNoAsync(2026);

            result.Should().Be(42);
        }
    }
}
