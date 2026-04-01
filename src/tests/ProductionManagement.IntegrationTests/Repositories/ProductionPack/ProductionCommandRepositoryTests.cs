using Contracts.Dtos.Lookups.Sales;
using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
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
    public sealed class ProductionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProductionCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── Mock factory ────────────────────────────────────────────────────

        private static Mock<ISalesMiscMasterLookup> BuildMiscLookup()
        {
            var mock = new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByCodeAsync("Packed"))
                .ReturnsAsync(new SalesMiscMasterLookupDto { Id = 1, Code = "Packed", Description = "Packed" });
            return mock;
        }

        private static Mock<ISalesStockLedgerService> BuildStockLedger()
        {
            var mock = new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            mock.Setup(x => x.InsertAsync(It.IsAny<List<SalesStockLedgerDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mock.Setup(x => x.DeleteByDocAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
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

        private ProductionCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, BuildMiscLookup().Object, BuildStockLedger().Object, BuildDocSeq().Object);

        // ── Prerequisites ───────────────────────────────────────────────────

        private async Task<(int miscTypeId, int lotId, int packTypeId, int qualityStatusId)>
            SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            var miscType = new Domain.Entities.MiscTypeMaster
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
            var qualityStatusId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
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

            var lotTypeId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LTYP",
                Description = "Lot Type",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var lotStatusId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
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

            return (miscType.Id, lotId, packTypeId, qualityStatusId);
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

        private static Domain.Entities.ProductionPackHeader BuildHeader(
            int lotId, int packTypeId, int qualityStatusId,
            string packNo = "PK-001",
            int unitId = 1,
            int warehouseId = 1) =>
            new()
            {
                PackNo = packNo,
                PackDate = new DateOnly(2026, 3, 1),
                ProductionYear = 2026,
                UnitId = unitId,
                WarehouseId = warehouseId,
                TotalBags = 5,
                TotalNetWeight = 125.0m,
                ProductionKgs = 130.0m,
                LooseConeKgs = 5.0m,
                Remarks = "Test production pack",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ProductionPackDetails = new List<Domain.Entities.ProductionPackDetail>
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
                        LineRemarks = "Detail line 1"
                    }
                }
            };

        // ── CREATE ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var header = BuildHeader(lotId, packTypeId, qsId);
            var newId = await CreateRepo(ctx).CreateAsync(header, typeId: 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var header = BuildHeader(lotId, packTypeId, qsId, packNo: "PK-002");
            var newId = await CreateRepo(ctx).CreateAsync(header, typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProductionPackHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PackNo.Should().Be("PK-002");
            saved.PackDate.Should().Be(new DateOnly(2026, 3, 1));
            saved.ProductionYear.Should().Be(2026);
            saved.UnitId.Should().Be(1);
            saved.WarehouseId.Should().Be(1);
            saved.TotalBags.Should().Be(5);
            saved.TotalNetWeight.Should().Be(125.0m);
            saved.ProductionKgs.Should().Be(130.0m);
            saved.LooseConeKgs.Should().Be(5.0m);
            saved.Remarks.Should().Be("Test production pack");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Detail_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var header = BuildHeader(lotId, packTypeId, qsId, packNo: "PK-003");
            var newId = await CreateRepo(ctx).CreateAsync(header, typeId: 1);
            ctx.ChangeTracker.Clear();

            var details = await ctx.ProductionPackDetail
                .Where(d => d.ProductionPackHeaderId == newId)
                .ToListAsync();

            details.Should().HaveCount(1);
            details[0].LotId.Should().Be(lotId);
            details[0].PackTypeId.Should().Be(packTypeId);
            details[0].QualityStatusId.Should().Be(qsId);
            details[0].NetWeightPerPack.Should().Be(25.0m);
            details[0].StartPackNo.Should().Be(1);
            details[0].EndPackNo.Should().Be(5);
            details[0].NoOfBags.Should().Be(5);
            details[0].TotalBags.Should().Be(5);
            details[0].TotalNetWeight.Should().Be(125.0m);
            details[0].BinId.Should().Be(1);
            details[0].LineRemarks.Should().Be("Detail line 1");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildHeader(lotId, packTypeId, qsId, "PK-004"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProductionPackHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_With_No_Details_Should_Still_Create_Header()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);

            var header = new Domain.Entities.ProductionPackHeader
            {
                PackNo = "PK-005",
                PackDate = new DateOnly(2026, 3, 1),
                ProductionYear = 2026,
                UnitId = 1,
                WarehouseId = 1,
                TotalBags = 0,
                TotalNetWeight = 0m,
                ProductionKgs = 0m,
                LooseConeKgs = 0m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ProductionPackDetails = null
            };

            var newId = await CreateRepo(ctx).CreateAsync(header, typeId: 1);

            newId.Should().BeGreaterThan(0);
        }

        // ── UPDATE ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildHeader(lotId, packTypeId, qsId, "PK-006"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.ProductionPackHeader
            {
                Id = newId,
                PackNo = "PK-006",
                PackDate = new DateOnly(2026, 4, 15),
                ProductionYear = 2026,
                UnitId = 1,
                WarehouseId = 2,
                TotalBags = 10,
                TotalNetWeight = 250.0m,
                ProductionKgs = 260.0m,
                LooseConeKgs = 10.0m,
                Remarks = "Updated remarks",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ProductionPackDetails = new List<Domain.Entities.ProductionPackDetail>
                {
                    new()
                    {
                        ItemSno = 1,
                        LotId = lotId,
                        ItemId = 1,
                        PackTypeId = packTypeId,
                        NetWeightPerPack = 25.0m,
                        StartPackNo = 1,
                        EndPackNo = 10,
                        NoOfBags = 10,
                        TotalBags = 10,
                        TotalNetWeight = 250.0m,
                        BinId = 1,
                        QualityStatusId = qsId,
                        LineRemarks = "Updated detail"
                    }
                }
            };

            var result = await CreateRepo(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(newId);

            var saved = await ctx.ProductionPackHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.PackDate.Should().Be(new DateOnly(2026, 4, 15));
            saved.WarehouseId.Should().Be(2);
            saved.TotalBags.Should().Be(10);
            saved.TotalNetWeight.Should().Be(250.0m);
            saved.Remarks.Should().Be("Updated remarks");
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Detail_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, lotId, packTypeId, qsId) = await SeedPrerequisitesAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildHeader(lotId, packTypeId, qsId, "PK-007"), typeId: 1);
            ctx.ChangeTracker.Clear();

            var updateEntity = new Domain.Entities.ProductionPackHeader
            {
                Id = newId,
                PackNo = "PK-007",
                PackDate = new DateOnly(2026, 3, 1),
                ProductionYear = 2026,
                UnitId = 1,
                WarehouseId = 1,
                TotalBags = 3,
                TotalNetWeight = 75.0m,
                ProductionKgs = 80.0m,
                LooseConeKgs = 5.0m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ProductionPackDetails = new List<Domain.Entities.ProductionPackDetail>
                {
                    new()
                    {
                        ItemSno = 1,
                        LotId = lotId,
                        ItemId = 1,
                        PackTypeId = packTypeId,
                        NetWeightPerPack = 25.0m,
                        StartPackNo = 1,
                        EndPackNo = 3,
                        NoOfBags = 3,
                        TotalBags = 3,
                        TotalNetWeight = 75.0m,
                        BinId = 1,
                        QualityStatusId = qsId,
                        LineRemarks = "Replaced detail"
                    }
                }
            };

            await CreateRepo(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var details = await ctx.ProductionPackDetail
                .Where(d => d.ProductionPackHeaderId == newId)
                .ToListAsync();

            details.Should().HaveCount(1);
            details[0].EndPackNo.Should().Be(3);
            details[0].LineRemarks.Should().Be("Replaced detail");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = new Domain.Entities.ProductionPackHeader
            {
                Id = 99999,
                PackNo = "GHOST",
                PackDate = new DateOnly(2026, 1, 1),
                ProductionYear = 2026,
                UnitId = 1,
                WarehouseId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }
    }
}
