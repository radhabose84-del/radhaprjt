using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
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
    public sealed class RepackingMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RepackingMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── Mock factories ──────────────────────────────────────────────────

        private static Mock<ISalesMiscMasterLookup> BuildMiscLookup()
        {
            var mock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1, Code = "Packed" });
            mock.Setup(x => x.GetByCodeAsync("Deleted"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 2, Code = "Deleted" });
            mock.Setup(x => x.GetByCodeAsync("Repacking"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 3, Code = "Repacking" });
            return mock;
        }

        private static Mock<ISalesStockLedgerService> BuildStockLedger(int lastPackNo = 0)
        {
            var mock = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            mock.Setup(x => x.InsertAsync(It.IsAny<List<SalesStockLedgerDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mock.Setup(x => x.DeleteByDocAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mock.Setup(x => x.UpdateStatusByPackRangeAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mock.Setup(x => x.GetLastPackNoByYearAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastPackNo);
            mock.Setup(x => x.GetLotIdByPackRangeAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
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

        private RepackingMasterCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<ISalesStockLedgerService>? stockLedger = null) =>
            new(ctx, BuildMiscLookup().Object, (stockLedger ?? BuildStockLedger()).Object, BuildDocSeq().Object);

        // ── Prerequisites ───────────────────────────────────────────────────

        private async Task<(int oldPackTypeId, int newPackTypeId, int looseHandlingId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            // MiscTypeMaster for loose handling
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

            // PackTypes (old and new)
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

        private static Domain.Entities.RepackingMaster BuildEntity(
            int oldPackTypeId, int newPackTypeId, int looseHandlingId,
            string docNo = "RPK-001",
            int unitId = 1,
            int warehouseId = 1) =>
            new()
            {
                UnitId = unitId,
                ProductionYear = 2026,
                RepackDocNo = docNo,
                RepackDate = new DateOnly(2026, 3, 15),
                ItemId = 1,
                OldPackTypeId = oldPackTypeId,
                OldNetWeightPerPack = 25.0m,
                OldStartPackNo = 1,
                OldEndPackNo = 5,
                OldTotalBags = 5,
                OldNetWeight = 125.0m,
                OldWarehouseId = warehouseId,
                OldBinId = 1,
                PackTypeId = newPackTypeId,
                NetWeightPerPack = 50.0m,
                StartPackNo = 0, // will be set by CreateAsync
                EndPackNo = 0,   // will be set by CreateAsync
                TotalBags = 3,
                NetWeight = 150.0m,
                WarehouseId = warehouseId,
                BinId = 2,
                LooseConeKgs = 5.0m,
                LooseHandlingId = looseHandlingId,
                Remarks = "Test repacking",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // ── CREATE ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(oldPt, newPt, lhId);
            var newId = await CreateRepo(ctx).CreateAsync(entity, typeId: 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(oldPt, newPt, lhId, docNo: "RPK-002");
            var newId = await CreateRepo(ctx).CreateAsync(entity, typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RepackingMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.RepackDocNo.Should().Be("RPK-002");
            saved.RepackDate.Should().Be(new DateOnly(2026, 3, 15));
            saved.ProductionYear.Should().Be(2026);
            saved.UnitId.Should().Be(1);
            saved.ItemId.Should().Be(1);
            saved.OldPackTypeId.Should().Be(oldPt);
            saved.OldNetWeightPerPack.Should().Be(25.0m);
            saved.OldStartPackNo.Should().Be(1);
            saved.OldEndPackNo.Should().Be(5);
            saved.OldTotalBags.Should().Be(5);
            saved.OldNetWeight.Should().Be(125.0m);
            saved.PackTypeId.Should().Be(newPt);
            saved.NetWeightPerPack.Should().Be(50.0m);
            saved.TotalBags.Should().Be(3);
            saved.NetWeight.Should().Be(150.0m);
            saved.LooseConeKgs.Should().Be(5.0m);
            saved.LooseHandlingId.Should().Be(lhId);
            saved.Remarks.Should().Be("Test repacking");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StartPackNo_And_EndPackNo()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);

            // Stock ledger returns lastPackNo = 10
            var stockMock = BuildStockLedger(lastPackNo: 10);
            var entity = BuildEntity(oldPt, newPt, lhId, docNo: "RPK-003");
            entity.TotalBags = 5;

            var newId = await CreateRepo(ctx, stockMock).CreateAsync(entity, typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RepackingMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.StartPackNo.Should().Be(11); // lastPackNo + 1
            saved.EndPackNo.Should().Be(15);    // startPackNo + totalBags - 1
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(oldPt, newPt, lhId, "RPK-004"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.RepackingMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UPDATE ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(oldPt, newPt, lhId, "RPK-005"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(oldPt, newPt, lhId, "RPK-005");
            updateEntity.Id = newId;
            updateEntity.Remarks = "Updated remarks";
            updateEntity.NetWeightPerPack = 75.0m;
            updateEntity.TotalBags = 4;
            updateEntity.NetWeight = 300.0m;

            var result = await CreateRepo(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(newId);

            var saved = await ctx.RepackingMaster.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.Remarks.Should().Be("Updated remarks");
            saved.NetWeightPerPack.Should().Be(75.0m);
            saved.NetWeight.Should().Be(300.0m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(1, 1, 1, "GHOST");
            entity.Id = 99999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // ── SOFT DELETE ─────────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(oldPt, newPt, lhId, "RPK-006"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(oldPt, newPt, lhId, "RPK-007"), typeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.RepackingMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_Deleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (oldPt, newPt, lhId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(oldPt, newPt, lhId, "RPK-008"), typeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
