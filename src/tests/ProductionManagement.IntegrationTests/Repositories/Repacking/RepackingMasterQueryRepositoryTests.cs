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
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.Infrastructure.Repositories.RepackingMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Repacking
{
    [Collection("DatabaseCollection")]
    public sealed class RepackingMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RepackingMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── Mock factories ──────────────────────────────────────────────────

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

        private RepackingMasterQueryRepository CreateQueryRepo(
            Mock<IItemLookup>? itemLookup = null,
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IBinLookup>? binLookup = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new RepackingMasterQueryRepository(
                conn,
                (itemLookup ?? BuildItemLookup()).Object,
                (unitLookup ?? BuildUnitLookup()).Object,
                (warehouseLookup ?? BuildWarehouseLookup()).Object,
                (binLookup ?? BuildBinLookup()).Object);
        }

        // ── Command repo for seeding ────────────────────────────────────────

        private static RepackingMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            miscMock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1 });
            miscMock.Setup(x => x.GetByCodeAsync("Deleted"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 2 });
            miscMock.Setup(x => x.GetByCodeAsync("Repacking"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 3 });

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
            stockMock.Setup(x => x.GetLotIdByPackRangeAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var docMock = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docMock.Setup(x => x.IncrementDocNoAsync(
                    It.IsAny<int>(),
                    It.IsAny<System.Data.IDbConnection>(),
                    It.IsAny<System.Data.IDbTransaction>()))
                .Returns(Task.CompletedTask);

            return new RepackingMasterCommandRepository(ctx, miscMock.Object, stockMock.Object, docMock.Object);
        }

        // ── Prerequisites ───────────────────────────────────────────────────

        private async Task<(int oldPackTypeId, int newPackTypeId, int looseHandlingId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            var miscType = new ProductionManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "LHTYP",
                Description = "Loose Handling Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var looseHandlingId = await miscRepo.CreateAsync(new ProductionManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LHWST",
                Description = "Waste",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var packTypeRepo = new PackTypeCommandRepository(ctx);
            var oldPtId = await packTypeRepo.CreateAsync(new Domain.Entities.PackType
            {
                PackTypeCode = "OLDPT",
                PackTypeName = "Old Pack Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var newPtId = await packTypeRepo.CreateAsync(new Domain.Entities.PackType
            {
                PackTypeCode = "NEWPT",
                PackTypeName = "New Pack Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            return (oldPtId, newPtId, looseHandlingId);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[RepackingMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[PackType]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        private async Task<int> SeedEntityAsync(
            ApplicationDbContext ctx,
            int oldPtId, int newPtId, int lhId,
            string docNo = "RPK-001")
        {
            var entity = new Domain.Entities.RepackingMaster
            {
                UnitId = 1,
                ProductionYear = 2026,
                RepackDocNo = docNo,
                RepackDate = new DateOnly(2026, 3, 15),
                ItemId = 1,
                OldPackTypeId = oldPtId,
                OldNetWeightPerPack = 25.0m,
                OldStartPackNo = 1,
                OldEndPackNo = 5,
                OldTotalBags = 5,
                OldNetWeight = 125.0m,
                OldWarehouseId = 1,
                OldBinId = 1,
                PackTypeId = newPtId,
                NetWeightPerPack = 50.0m,
                StartPackNo = 0,
                EndPackNo = 0,
                TotalBags = 3,
                NetWeight = 150.0m,
                WarehouseId = 2,
                BinId = 2,
                LooseConeKgs = 5.0m,
                LooseHandlingId = lhId,
                Remarks = "Test repacking",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            return await CreateCommandRepo(ctx).CreateAsync(entity, typeId: 1);
        }

        // ── GET ALL ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CrossModule_Names()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            var itemMock = BuildItemLookup(1, "Cotton Yarn");
            var unitMock = BuildUnitLookup(1, "Spinning Unit");
            var whMock = BuildWarehouseLookup(1, "Old WH", 2, "New WH");
            var binMock = BuildBinLookup(1, "Old Bin", 2, "New Bin");

            var (items, _) = await CreateQueryRepo(itemMock, unitMock, whMock, binMock).GetAllAsync(1, 10, null);

            items[0].ItemName.Should().Be("Cotton Yarn");
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
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            // PackType and LooseHandling names come from same-module JOINs
            items[0].OldPackTypeName.Should().Be("Old Pack Type");
            items[0].PackTypeName.Should().Be("New Pack Type");
            items[0].LooseHandlingName.Should().Be("Waste");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsDeleted = 1 WHERE Id = {0}", id);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "ALPHA-RPK");
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "BETA-RPK");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].RepackDocNo.Should().Be("ALPHA-RPK");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "RPK-P1");
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "RPK-P2");
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "RPK-P3");

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 2, null);

            total.Should().Be(3);
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(1);
        }

        // ── GET BY ID ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId, "RPK-GBI");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.RepackDocNo.Should().Be("RPK-GBI");
            dto.OldPackTypeName.Should().Be("Old Pack Type");
            dto.PackTypeName.Should().Be("New Pack Type");
            dto.LooseHandlingName.Should().Be("Waste");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsDeleted = 1 WHERE Id = {0}", id);

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
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "AUTO-RPK");

            var results = await CreateQueryRepo().AutocompleteAsync("AUTO", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].RepackDocNo.Should().Be("AUTO-RPK");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId, "INACT-RPK");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsActive = 0 WHERE Id = {0}", id);

            var results = await CreateQueryRepo().AutocompleteAsync("INACT", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId, "DEL-RPK");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsDeleted = 1 WHERE Id = {0}", id);

            var results = await CreateQueryRepo().AutocompleteAsync("DEL", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // ── ALREADY EXISTS ──────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            await SeedEntityAsync(ctx, oldPt, newPt, lhId, "DUP-RPK");

            var result = await CreateQueryRepo().AlreadyExistsAsync("DUP-RPK");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Unique()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateQueryRepo().AlreadyExistsAsync("UNIQUE-RPK");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId, "SELF-RPK");

            var result = await CreateQueryRepo().AlreadyExistsAsync("SELF-RPK", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId, "SDDEL-RPK");

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsDeleted = 1 WHERE Id = {0}", id);

            var result = await CreateQueryRepo().AlreadyExistsAsync("SDDEL-RPK");

            result.Should().BeFalse();
        }

        // ── NOT FOUND ───────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId);

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
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedEntityAsync(ctx, oldPt, newPt, lhId);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.RepackingMaster SET IsDeleted = 1 WHERE Id = {0}", id);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── SAME-MODULE FK VALIDATION ───────────────────────────────────────

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_True_When_Active()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, _, _) = await SeedPrerequisitesAsync(ctx);

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
            var (oldPt, _, _) = await SeedPrerequisitesAsync(ctx);

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
            var (_, _, lhId) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateQueryRepo().MiscMasterExistsAsync(lhId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateQueryRepo().MiscMasterExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, _, lhId) = await SeedPrerequisitesAsync(ctx);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Production.MiscMaster SET IsActive = 0 WHERE Id = {0}", lhId);

            var result = await CreateQueryRepo().MiscMasterExistsAsync(lhId);

            result.Should().BeFalse();
        }
    }
}
