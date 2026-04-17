using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.BillEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BillEntry
{
    [Collection("DatabaseCollection")]
    public sealed class BillEntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public BillEntryQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseBillEntryQueryRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string billNumber = "BQ001", int partyId = 100, int detailCount = 1)
        {
            var entity = new PurchaseBillEntryHeader
            {
                UnitId = 1,
                BillNumber = billNumber,
                BillDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                PartyId = partyId,
                POCategoryId = 1,
                POMethodId = 1,
                SubTotal = 500m,
                TaxableAmount = 475m,
                GrandTotal = 522m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Lines = Enumerable.Range(1, detailCount).Select(i => new PurchaseBillEntryDetail
                {
                    PoDetailId = i,
                    ItemId = i * 10,
                    PoQty = 50m,
                    GrnQty = 50m,
                    BilledQty = 50m,
                    PoRate = 10m,
                    BilledRate = 10m,
                    LineBaseAmount = 500m,
                    TaxableAmount = 475m,
                    LineTotal = 522m,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };
            await ctx.Set<PurchaseBillEntryHeader>().AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
                DELETE FROM [Purchase].[PurchaseBillEntryDetail];
                DELETE FROM [Purchase].[PurchaseBillEntryHeader];");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Lines()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await SeedAsync(ctx, "BQ_G1", detailCount: 3);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Lines.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Seeded()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "BQ_L1");

            var (rows, total) = await CreateRepo(ctx).GetListAsync(null, null, null, null, 1, 10);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_PartyId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "BQ_FP1", partyId: 100);
            await SeedAsync(ctx, "BQ_FP2", partyId: 200);

            var (rows, total) = await CreateRepo(ctx).GetListAsync(partyId: 100, null, null, null, 1, 10);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Search()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "UNIQUE_BQ_Z");
            await SeedAsync(ctx, "OTHER_BQ");

            var (rows, _) = await CreateRepo(ctx).GetListAsync(null, "UNIQUE_BQ_Z", null, null, 1, 10);

            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAutoCompleteAsync_Should_Return_Matching()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "BQ_AC_MATCH");

            var result = await CreateRepo(ctx).GetAutoCompleteAsync("BQ_AC", 20);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task BillNumberExistsAsync_Should_Return_True_For_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "BQ_DUP", partyId: 100);

            var result = await CreateRepo(ctx).BillNumberExistsAsync(100, "BQ_DUP", null);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task BillNumberExistsAsync_Should_Exclude_Self()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await SeedAsync(ctx, "BQ_SELF", partyId: 100);

            var result = await CreateRepo(ctx).BillNumberExistsAsync(100, "BQ_SELF", excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task BillNumberExistsAsync_Should_Return_False_For_Different_Party()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await SeedAsync(ctx, "BQ_PARTY", partyId: 100);

            var result = await CreateRepo(ctx).BillNumberExistsAsync(999, "BQ_PARTY", null);

            result.Should().BeFalse();
        }
    }
}
