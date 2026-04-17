using Contracts.Dtos.Stock;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.RepackingHeader;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.RepackingHeader
{
    [Collection("DatabaseCollection")]
    public sealed class RepackingHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public RepackingHeaderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RepackingHeaderCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<ISalesMiscMasterLookup>? salesMisc = null,
            Mock<ISalesStockLedgerService>? stockLedger = null,
            Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            salesMisc ??= new Mock<ISalesMiscMasterLookup>(MockBehavior.Loose);
            stockLedger ??= new Mock<ISalesStockLedgerService>(MockBehavior.Loose);
            docSeq ??= new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);

            stockLedger.Setup(s => s.UpdateStatusByPackRangeAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockLedger.Setup(s => s.DeleteByDocAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            stockLedger.Setup(s => s.GetLotIdByPackRangeAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            return new RepackingHeaderCommandRepository(ctx, salesMisc.Object, stockLedger.Object, docSeq.Object);
        }

        private async Task<int> EnsureLotIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "RHC_LM_T");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "RHC_LM_T", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "RHC_LM_M");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "RHC_LM_M", Description = "M", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            var existingLot = await ctx.LotMaster.FirstOrDefaultAsync(x => x.LotCode == "RHC_LOT");
            if (existingLot != null) return existingLot.Id;
            var lm = new Domain.Entities.LotMaster
            {
                LotCode = "RHC_LOT", BatchNumber = "B",
                LotTypeId = m.Id, StatusId = m.Id,
                ItemId = 1, UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.LotMaster.AddAsync(lm);
            await ctx.SaveChangesAsync();
            return lm.Id;
        }

        private async Task<int> EnsurePackTypeIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.PackType.FirstOrDefaultAsync(x => x.PackTypeCode == "RHC_PT");
            if (existing != null) return existing.Id;
            var pt = new Domain.Entities.PackType
            {
                PackTypeCode = "RHC_PT", PackTypeName = "RHC_PT",
                NetWeight = 1m, TareWeight = 0m, GrossWeight = 1m,
                ConesPerBag = 1, ProductionAllowed = true,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.PackType.AddAsync(pt);
            await ctx.SaveChangesAsync();
            return pt.Id;
        }

        private async Task<int> SeedRepackingHeaderAsync(
            string docNo = "RH_C1",
            IsDelete deleted = IsDelete.NotDeleted,
            bool withDetails = false)
        {
            var lotId = await EnsureLotIdAsync();
            var ptId = await EnsurePackTypeIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var rh = new Domain.Entities.RepackingHeader
            {
                UnitId = 1,
                ProductionYear = DateTime.UtcNow.Year,
                RepackDocNo = docNo,
                RepackDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ItemId = 1, OldItemId = 1, // isRepacking = true
                PackTypeId = ptId, OldPackTypeId = ptId,
                StartPackNo = 0, EndPackNo = 0,
                NetWeightPerPack = 0m,
                TotalBags = 0, NetWeight = 0m,
                WarehouseId = 1, BinId = 0,
                LooseConeKgs = 0m, WasteQuantity = 0m,
                LotId = lotId,
                IsActive = Status.Active, IsDeleted = deleted,
                RepackingDetails = withDetails
                    ? new List<Domain.Entities.RepackingDetail>
                    {
                        new() { OldStartPackNo = 1, OldEndPackNo = 5 }
                    }
                    : null
            };
            await ctx.RepackingHeader.AddAsync(rh);
            await ctx.SaveChangesAsync();
            return rh.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = new Domain.Entities.RepackingHeader
            {
                Id = 9999999,
                UnitId = 1, ItemId = 1, OldItemId = 1,
                ProductionYear = DateTime.UtcNow.Year,
                RepackDate = DateOnly.FromDateTime(DateTime.UtcNow),
                RepackDocNo = "GH",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_SoftDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await SeedRepackingHeaderAsync("RH_USD", deleted: IsDelete.Deleted);

            var entity = new Domain.Entities.RepackingHeader
            {
                Id = id,
                UnitId = 1, ItemId = 1, OldItemId = 1,
                ProductionYear = DateTime.UtcNow.Year,
                RepackDate = DateOnly.FromDateTime(DateTime.UtcNow),
                RepackDocNo = "RH_USD",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SoftDeleteAsync ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag_When_Header_Has_No_Details()
        {
            await ClearAsync();
            var id = await SeedRepackingHeaderAsync("RH_SD1", withDetails: false);
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.RepackingHeader.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedRepackingHeaderAsync("RH_SD2", deleted: IsDelete.Deleted);
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
