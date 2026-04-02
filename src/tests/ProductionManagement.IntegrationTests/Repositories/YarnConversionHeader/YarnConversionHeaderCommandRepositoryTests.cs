using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
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
    public sealed class YarnConversionHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public YarnConversionHeaderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // -- Mock factories --

        private static Mock<ISalesMiscMasterLookup> BuildMiscLookup()
        {
            var mock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1, Code = "Packed" });
            mock.Setup(x => x.GetByCodeAsync("Deleted"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 2, Code = "Deleted" });
            mock.Setup(x => x.GetByCodeAsync("YarnConversion"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 4, Code = "YarnConversion" });
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

        private YarnConversionHeaderCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<ISalesStockLedgerService>? stockLedger = null) =>
            new(ctx, BuildMiscLookup().Object, (stockLedger ?? BuildStockLedger()).Object, BuildDocSeq().Object);

        // -- Prerequisites --

        private async Task<(int lotId, int oldPackTypeId, int newPackTypeId, int faultId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            // MiscTypeMaster for fault
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

            // PackTypes
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

            // LotMaster
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

        private static ProductionManagement.Domain.Entities.YarnConversionHeader BuildEntity(
            int lotId, int oldPackTypeId, int newPackTypeId, int? faultId = null,
            string docNo = "YC-001",
            int unitId = 1) =>
            new()
            {
                UnitId = unitId,
                ProductionYear = 2026,
                ConversionDocNo = docNo,
                ConversionDate = new DateOnly(2026, 3, 20),
                LotId = lotId,
                OldItemId = 1,
                OldPackTypeId = oldPackTypeId,
                OldStartPackNo = 1,
                OldEndPackNo = 10,
                OldTotalBags = 10,
                OldNetWeightPerPack = 25.0m,
                OldNetWeight = 250.0m,
                OldWarehouseId = 1,
                OldBinId = 1,
                FaultId = faultId,
                ItemId = 2,
                PackTypeId = newPackTypeId,
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

        // -- CREATE --

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(lotId, oldPt, newPt, faultId);
            var newId = await CreateRepo(ctx).CreateAsync(entity, typeId: 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, faultId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(lotId, oldPt, newPt, faultId, docNo: "YC-002");
            var newId = await CreateRepo(ctx).CreateAsync(entity, typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.YarnConversionHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ConversionDocNo.Should().Be("YC-002");
            saved.ConversionDate.Should().Be(new DateOnly(2026, 3, 20));
            saved.ProductionYear.Should().Be(2026);
            saved.UnitId.Should().Be(1);
            saved.LotId.Should().Be(lotId);
            saved.OldItemId.Should().Be(1);
            saved.OldPackTypeId.Should().Be(oldPt);
            saved.ItemId.Should().Be(2);
            saved.PackTypeId.Should().Be(newPt);
            saved.FaultId.Should().Be(faultId);
            saved.TotalBags.Should().Be(5);
            saved.NetWeightPerPack.Should().Be(50.0m);
            saved.NetWeight.Should().Be(250.0m);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StartPackNo_And_EndPackNo()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);

            var stockMock = BuildStockLedger(lastPackNo: 20);
            var entity = BuildEntity(lotId, oldPt, newPt, docNo: "YC-003");
            entity.TotalBags = 3;

            var newId = await CreateRepo(ctx, stockMock).CreateAsync(entity, typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.YarnConversionHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.StartPackNo.Should().Be(21);
            saved.EndPackNo.Should().Be(23);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, oldPt, newPt, docNo: "YC-004"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.YarnConversionHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // -- UPDATE --

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, oldPt, newPt, docNo: "YC-005"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var updateEntity = BuildEntity(lotId, oldPt, newPt, docNo: "YC-005");
            updateEntity.Id = newId;
            updateEntity.Remarks = "Updated remarks";
            updateEntity.NetWeightPerPack = 75.0m;

            var result = await CreateRepo(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(newId);

            var saved = await ctx.YarnConversionHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.Remarks.Should().Be("Updated remarks");
            saved.NetWeightPerPack.Should().Be(75.0m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(1, 1, 1, docNo: "GHOST");
            entity.Id = 99999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // -- SOFT DELETE --

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, oldPt, newPt, docNo: "YC-006"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, oldPt, newPt, docNo: "YC-007"), typeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.YarnConversionHeader
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
            var (lotId, oldPt, newPt, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, oldPt, newPt, docNo: "YC-008"), typeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
