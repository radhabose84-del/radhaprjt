using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.Infrastructure.Repositories.YarnConversionHeader;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.YarnConversionHeader
{
    [Collection("DatabaseCollection")]
    public sealed class YarnConversionHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public YarnConversionHeaderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // -- Mock factories --

        private static Mock<IItemLookup> BuildItemLookup(int id = 1, string name = "Test Item")
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new() { Id = id, ItemName = name }
                });
            return mock;
        }

        private static Mock<IUnitLookup> BuildUnitLookup(int id = 1, string name = "Test Unit")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = id, UnitName = name }
                });
            return mock;
        }

        private static Mock<IWarehouseLookup> BuildWarehouseLookup(
            int id1 = 1, string name1 = "Warehouse 1",
            int id2 = 2, string name2 = "Warehouse 2")
        {
            var mock = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>
                {
                    new() { Id = id1, WarehouseName = name1 },
                    new() { Id = id2, WarehouseName = name2 }
                });
            return mock;
        }

        private static Mock<IBinLookup> BuildBinLookup(
            int id1 = 1, string name1 = "Bin 1",
            int id2 = 2, string name2 = "Bin 2")
        {
            var mock = new Mock<IBinLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>
                {
                    new() { Id = id1, BinName = name1 },
                    new() { Id = id2, BinName = name2 }
                });
            return mock;
        }

        private YarnConversionHeaderQueryRepository CreateQueryRepo(
            Mock<IItemLookup>? itemLookup = null,
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IBinLookup>? binLookup = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new YarnConversionHeaderQueryRepository(
                conn,
                (itemLookup ?? BuildItemLookup()).Object,
                (unitLookup ?? BuildUnitLookup()).Object,
                (warehouseLookup ?? BuildWarehouseLookup()).Object,
                (binLookup ?? BuildBinLookup()).Object);
        }

        // -- Command repo for seeding --

        private static YarnConversionHeaderCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            miscMock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1 });
            miscMock.Setup(x => x.GetByCodeAsync("Deleted"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 2 });
            miscMock.Setup(x => x.GetByCodeAsync("YarnConversion"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 4 });

            var stockMock = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            stockMock.Setup(x => x.InsertAsync(It.IsAny<List<SalesStockLedgerDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockMock.Setup(x => x.UpdateStatusByPackRangeAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockMock.Setup(x => x.DeleteByDocAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockMock.Setup(x => x.GetLastPackNoByYearAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var docMock = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docMock.Setup(x => x.IncrementDocNoAsync(
                    It.IsAny<int>(),
                    It.IsAny<System.Data.IDbConnection>(),
                    It.IsAny<System.Data.IDbTransaction>()))
                .Returns(Task.CompletedTask);

            return new YarnConversionHeaderCommandRepository(ctx, miscMock.Object, stockMock.Object, docMock.Object);
        }

        // -- Prerequisites --

        private async Task<(int lotId, int oldPackTypeId, int newPackTypeId, int faultId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            var miscType = new ProductionManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "FAULT",
                Description = "Fault Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var faultId = await miscRepo.CreateAsync(new ProductionManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "FLTST",
                Description = "Test Fault",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var packTypeRepo = new PackTypeCommandRepository(ctx);
            var oldPtId = await packTypeRepo.CreateAsync(new ProductionManagement.Domain.Entities.PackType
            {
                PackTypeCode = "YCOLD",
                PackTypeName = "YC Old Pack",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var newPtId = await packTypeRepo.CreateAsync(new ProductionManagement.Domain.Entities.PackType
            {
                PackTypeCode = "YCNEW",
                PackTypeName = "YC New Pack",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var lotRepo = new LotMasterCommandRepository(ctx);
            var lotId = await lotRepo.CreateAsync(new ProductionManagement.Domain.Entities.LotMaster
            {
                LotCode = "LOT001",
                LotTypeId = faultId, // reuse misc as lot type
                ItemId = 1,
                UnitId = 1,
                StartDate = new DateOnly(2026, 1, 1),
                StatusId = faultId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            return (lotId, oldPtId, newPtId, faultId);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[YarnConversionHeader]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[LotMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[PackType]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private async Task<int> SeedEntityAsync(
            ApplicationDbContext ctx,
            int lotId, int oldPtId, int newPtId, int? faultId = null,
            string docNo = "YC-001")
        {
            var entity = new ProductionManagement.Domain.Entities.YarnConversionHeader
            {
                UnitId = 1,
                ProductionYear = 2026,
                ConversionDocNo = docNo,
                ConversionDate = new DateOnly(2026, 3, 20),
                LotId = lotId,
                OldItemId = 1,
                OldPackTypeId = oldPtId,
                OldStartPackNo = 1,
                OldEndPackNo = 10,
                OldTotalBags = 10,
                OldNetWeightPerPack = 25.0m,
                OldNetWeight = 250.0m,
                OldWarehouseId = 1,
                OldBinId = 1,
                FaultId = faultId,
                ItemId = 2,
                PackTypeId = newPtId,
                TotalBags = 5,
                NetWeightPerPack = 50.0m,
                NetWeight = 250.0m,
                StartPackNo = 0,
                EndPackNo = 0,
                LooseQty = 0m,
                WarehouseId = 2,
                BinId = 2,
                WasteQty = 0m,
                Remarks = "Test yarn conversion",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            return await CreateCommandRepo(ctx).CreateAsync(entity, typeId: 1);
        }

        // -- GET ALL --

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, faultId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CrossModule_Names()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, faultId);

            var itemMock = BuildItemLookup(1, "Cotton Yarn");
            var unitMock = BuildUnitLookup(1, "Spinning Unit");
            var whMock = BuildWarehouseLookup(1, "Old WH", 2, "New WH");
            var binMock = BuildBinLookup(1, "Old Bin", 2, "New Bin");

            var (items, _) = await CreateQueryRepo(itemMock, unitMock, whMock, binMock).GetAllAsync(1, 10, null);

            items[0].UnitName.Should().Be("Spinning Unit");
            items[0].OldWarehouseName.Should().Be("Old WH");
            items[0].WarehouseName.Should().Be("New WH");
            items[0].OldBinName.Should().Be("Old Bin");
            items[0].BinName.Should().Be("New Bin");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_SameModule_JoinNames()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, faultId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].OldPackTypeName.Should().Be("YC Old Pack");
            items[0].PackTypeName.Should().Be("YC New Pack");
            items[0].LotName.Should().Be("LOT001");
            items[0].FaultName.Should().Be("Test Fault");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.YarnConversionHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, docNo: "ALPHA-YC");
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, docNo: "BETA-YC");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].ConversionDocNo.Should().Be("ALPHA-YC");
        }

        // -- GET BY ID --

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt, faultId, "YC-GBI");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ConversionDocNo.Should().Be("YC-GBI");
            dto.OldPackTypeName.Should().Be("YC Old Pack");
            dto.PackTypeName.Should().Be("YC New Pack");
            dto.LotName.Should().Be("LOT001");
            dto.FaultName.Should().Be("Test Fault");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.YarnConversionHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // -- AUTOCOMPLETE --

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, lotId, oldPt, newPt, docNo: "AUTO-YC");

            var results = await CreateQueryRepo().AutocompleteAsync("AUTO", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ConversionDocNo.Should().Be("AUTO-YC");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt, docNo: "INACT-YC");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.YarnConversionHeader SET IsActive = 0 WHERE Id = {0}", id);

            var results = await CreateQueryRepo().AutocompleteAsync("INACT", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt, docNo: "DEL-YC");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.YarnConversionHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var results = await CreateQueryRepo().AutocompleteAsync("DEL", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // -- NOT FOUND --

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt);

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
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, lotId, oldPt, newPt);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.YarnConversionHeader SET IsDeleted = 1 WHERE Id = {0}", id);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // -- SAME-MODULE FK VALIDATION --

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_True_When_Active()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, oldPt, _, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().PackTypeExistsAsync(oldPt);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().PackTypeExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_False_When_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, oldPt, _, _) = await SeedPrerequisitesAsync(ctx);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.PackType SET IsActive = 0 WHERE Id = {0}", oldPt);

            var result = await CreateQueryRepo().PackTypeExistsAsync(oldPt);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Active()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, _, _, faultId) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().MiscMasterExistsAsync(faultId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().MiscMasterExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task LotMasterExistsAsync_Should_Return_True_When_Active()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, _, _, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().LotMasterExistsAsync(lotId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task LotMasterExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().LotMasterExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task LotMasterExistsAsync_Should_Return_False_When_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, _, _, _) = await SeedPrerequisitesAsync(ctx);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.LotMaster SET IsActive = 0 WHERE Id = {0}", lotId);

            var result = await CreateQueryRepo().LotMasterExistsAsync(lotId);

            result.Should().BeFalse();
        }
    }
}
