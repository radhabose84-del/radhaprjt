using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.RepackingHeader;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.RepackingHeader
{
    [Collection("DatabaseCollection")]
    public sealed class RepackingHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public RepackingHeaderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RepackingHeaderQueryRepository CreateRepo(
            Mock<IUnitLookup>? unit = null,
            Mock<IItemLookup>? item = null,
            Mock<IWarehouseLookup>? wh = null,
            Mock<IBinLookup>? bin = null,
            Mock<ISalesStockLedgerService>? stockLedger = null)
        {
            unit ??= new Mock<IUnitLookup>(MockBehavior.Loose);
            item ??= new Mock<IItemLookup>(MockBehavior.Loose);
            wh ??= new Mock<IWarehouseLookup>(MockBehavior.Loose);
            bin ??= new Mock<IBinLookup>(MockBehavior.Loose);
            stockLedger ??= new Mock<ISalesStockLedgerService>(MockBehavior.Loose);

            unit.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto>());
            item.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());
            wh.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<WarehouseLookupDto>)new List<WarehouseLookupDto>());
            bin.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());

            return new RepackingHeaderQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unit.Object, item.Object, wh.Object, bin.Object, stockLedger.Object);
        }

        private async Task<int> EnsureLotIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "RHQ_LM_T");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "RHQ_LM_T", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "RHQ_LM_M");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "RHQ_LM_M", Description = "M", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            var existingLot = await ctx.LotMaster.FirstOrDefaultAsync(x => x.LotCode == "RHQ_LOT");
            if (existingLot != null) return existingLot.Id;
            var lm = new Domain.Entities.LotMaster
            {
                LotCode = "RHQ_LOT", BatchNumber = "B",
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
            var existing = await ctx.PackType.FirstOrDefaultAsync(x => x.PackTypeCode == "RHQ_PT");
            if (existing != null) return existing.Id;
            var pt = new Domain.Entities.PackType
            {
                PackTypeCode = "RHQ_PT", PackTypeName = "RHQ_PT",
                NetWeight = 1m, TareWeight = 0m, GrossWeight = 1m,
                ConesPerBag = 1, ProductionAllowed = true,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.PackType.AddAsync(pt);
            await ctx.SaveChangesAsync();
            return pt.Id;
        }

        private async Task<int> SeedHeaderAsync(string docNo, IsDelete deleted = IsDelete.NotDeleted)
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
                ItemId = 1, OldItemId = 1,
                PackTypeId = ptId, OldPackTypeId = ptId,
                StartPackNo = 0, EndPackNo = 0,
                NetWeightPerPack = 0m,
                TotalBags = 0, NetWeight = 0m,
                WarehouseId = 1, BinId = 0,
                LooseConeKgs = 0m, WasteQuantity = 0m,
                LotId = lotId,
                IsActive = Status.Active, IsDeleted = deleted
            };
            await ctx.RepackingHeader.AddAsync(rh);
            await ctx.SaveChangesAsync();
            return rh.Id;
        }

        private async Task SeedDetailAsync(int repackHeaderId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.RepackingDetail.AddAsync(new Domain.Entities.RepackingDetail
            {
                RepackHeaderId = repackHeaderId,
                OldStartPackNo = 1, OldEndPackNo = 5
            });
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

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
            var id = await SeedHeaderAsync("RHQ_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().PackTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task LotMasterExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().LotMasterExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("RHQ_SDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Has_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("RHQ_SDV2");
            await SeedDetailAsync(id);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsRepackingHeaderLinkedAsync_Should_Return_False_When_No_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("RHQ_LK1");

            var result = await CreateRepo().IsRepackingHeaderLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedHeaderAsync("RHQ_AC1");

            var result = await CreateRepo().AutocompleteAsync("RHQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
