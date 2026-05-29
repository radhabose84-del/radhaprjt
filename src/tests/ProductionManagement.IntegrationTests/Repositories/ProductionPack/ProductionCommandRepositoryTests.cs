using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Infrastructure.Data;
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

        private ProductionCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<ISalesMiscMasterLookup>? salesMisc = null,
            Mock<ISalesStockLedgerService>? stockLedger = null,
            Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            salesMisc ??= new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            stockLedger ??= new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            docSeq ??= new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);

            stockLedger.Setup(s => s.InsertAsync(It.IsAny<List<SalesStockLedgerDto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockLedger.Setup(s => s.DeleteByDocAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<System.Data.IDbConnection>(), It.IsAny<System.Data.IDbTransaction>()))
                .Returns(Task.CompletedTask);

            return new ProductionCommandRepository(ctx, salesMisc.Object, stockLedger.Object, docSeq.Object);
        }

        private async Task<int> SeedLotMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PPC_LM_T");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PPC_LM_T", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PPC_LM_M");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "PPC_LM_M", Description = "M", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            var lm = new Domain.Entities.LotMaster
            {
                LotCode = $"LOTC_{Guid.NewGuid():N}".Substring(0, 20),
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

        private static ProductionPackEntry BuildEntity(int lotId, string packNo = "PCMD1") =>
            new()
            {
                PackNo = packNo,
                PackDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ProductionYear = DateTime.UtcNow.Year,
                UnitId = 1, WarehouseId = 1,
                ItemId = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                Details = new List<ProductionPackEntryDetail>
                {
                    new ProductionPackEntryDetail
                    {
                        LotId = lotId,
                        TotalBags = 0,
                        TotalNetWeight = 0m,
                        OpeningLooseKgs = 0m,
                        TotalProductionKgs = 0m,
                        ProductionKgs = 0m,
                        LooseConeKgs = 0m
                    }
                }
            };

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Without_PackRange_Should_Persist_And_Return_NewId()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // No StartPackNo/EndPackNo so the cross-module InsertAsync stock-ledger branch is skipped
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(lotId, "PCMD_1"), typeId: 1);
            ctx.ChangeTracker.Clear();

            id.Should().BeGreaterThan(0);
            var saved = await ctx.ProductionPackEntry.FirstAsync(x => x.Id == id);
            saved.PackNo.Should().Be("PCMD_1");
        }

        [Fact]
        public async Task CreateAsync_Should_Insert_ProductionStockLedger_Row()
        {
            await ClearAsync();
            var lotId = await SeedLotMasterAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(lotId, "PCMD_2");
            var detail = entity.Details!.First();
            detail.TotalNetWeight = 100m;
            detail.TotalBags = 5;
            var id = await CreateRepo(ctx).CreateAsync(entity, typeId: 1);
            ctx.ChangeTracker.Clear();

            id.Should().BeGreaterThan(0);
            var ledger = await ctx.ProductionStockLedger.FirstOrDefaultAsync(
                l => l.UnitId == 1 && l.ItemId == 1 && l.LotId == lotId);
            ledger.Should().NotBeNull();
            ledger!.TotalBags.Should().Be(5);
            ledger.NetWeight.Should().Be(100m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = BuildEntity(1, "GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }
    }
}
