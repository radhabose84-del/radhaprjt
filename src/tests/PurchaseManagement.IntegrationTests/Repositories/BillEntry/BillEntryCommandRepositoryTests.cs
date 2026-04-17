using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BillEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BillEntry
{
    [Collection("DatabaseCollection")]
    public sealed class BillEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public BillEntryCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseBillEntryCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private PurchaseBillEntryHeader BuildEntity(string billNumber = "BE001", int detailCount = 2) =>
            new()
            {
                UnitId = 1,
                BillNumber = billNumber,
                BillDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PartyId = 100,
                POCategoryId = 1,
                POMethodId = 1,
                SubTotal = 1000m,
                DiscountTotal = 50m,
                TaxableAmount = 950m,
                CgstAmount = 47.5m,
                SgstAmount = 47.5m,
                IgstAmount = 0m,
                OtherCharges = 0m,
                RoundOff = 0m,
                GrandTotal = 1045m,
                Remarks = "test",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Lines = Enumerable.Range(1, detailCount).Select(i => new PurchaseBillEntryDetail
                {
                    PoDetailId = i,
                    ItemId = i * 10,
                    PoQty = 100m,
                    GrnQty = 100m,
                    BilledQty = 100m,
                    PoRate = 10m,
                    BilledRate = 10m,
                    TaxPercentage = 10m,
                    LineBaseAmount = 1000m,
                    TaxableAmount = 950m,
                    LineTotal = 1045m,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };

        private async Task ClearAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
                DELETE FROM [Purchase].[PurchaseBillEntryDetail];
                DELETE FROM [Purchase].[PurchaseBillEntryHeader];");
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Header_And_Lines()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = BuildEntity("BE_C1", detailCount: 3);
            await CreateRepo(ctx).AddAsync(entity);

            entity.Id.Should().BeGreaterThan(0);
            var lines = await ctx.Set<PurchaseBillEntryDetail>()
                .Where(d => d.BillEntryHeaderId == entity.Id).ToListAsync();
            lines.Should().HaveCount(3);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = BuildEntity("BE_C2");
            await CreateRepo(ctx).AddAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseBillEntryHeader>().FirstAsync(x => x.Id == entity.Id);
            saved.BillNumber.Should().Be("BE_C2");
            saved.PartyId.Should().Be(100);
            saved.GrandTotal.Should().Be(1045m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = BuildEntity("BE_U1");
            await CreateRepo(ctx).AddAsync(entity);
            ctx.ChangeTracker.Clear();

            var toUpdate = await ctx.Set<PurchaseBillEntryHeader>().FirstAsync(x => x.Id == entity.Id);
            toUpdate.Remarks = "updated";
            toUpdate.GrandTotal = 2000m;
            await CreateRepo(ctx).UpdateAsync(toUpdate);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.Set<PurchaseBillEntryHeader>().FirstAsync(x => x.Id == entity.Id);
            reloaded.Remarks.Should().Be("updated");
            reloaded.GrandTotal.Should().Be(2000m);
        }

        [Fact]
        public async Task DeleteByGrnIdAsync_Should_Remove_Matching_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var entity = BuildEntity("BE_D1");
            entity.GrnId = 42;
            await CreateRepo(ctx).AddAsync(entity);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteByGrnIdAsync(42);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.Set<PurchaseBillEntryHeader>()
                .Where(h => h.GrnId == 42).ToListAsync();
            remaining.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteByGrnIdAsync_Should_Not_Affect_Other_GrnIds()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var e1 = BuildEntity("BE_D2"); e1.GrnId = 50;
            var e2 = BuildEntity("BE_D3"); e2.GrnId = 51;
            await CreateRepo(ctx).AddAsync(e1);
            await CreateRepo(ctx).AddAsync(e2);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteByGrnIdAsync(50);
            ctx.ChangeTracker.Clear();

            (await ctx.Set<PurchaseBillEntryHeader>().CountAsync()).Should().Be(1);
        }
    }
}
