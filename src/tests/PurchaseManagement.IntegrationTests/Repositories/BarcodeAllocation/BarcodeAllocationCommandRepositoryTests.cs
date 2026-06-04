using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.BarcodeAllocation;
using PurchaseManagement.Infrastructure.Repositories.BarcodeSeries;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BarcodeAllocation
{
    [Collection("DatabaseCollection")]
    public sealed class BarcodeAllocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IFinancialYearLookup> _fyMock = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscMock = new(MockBehavior.Loose);

        private static readonly DateTimeOffset GenDate = new(2025, 6, 3, 0, 0, 0, TimeSpan.Zero);

        public BarcodeAllocationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _fyMock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new() { FinancialYearId = 1, StartYear = "2025", StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2026, 3, 31), IsActive = true }
            });
        }

        private BarcodeAllocationCommandRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, _fyMock.Object, _miscMock.Object);

        private sealed record Seeded(int PrefixId, int SeriesOpenId, int SeriesPartialId, int SeriesFullId);

        private async Task<Seeded> SeedMiscAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var miscRepo = new MiscMasterCommandRepository(ctx);

            async Task<int> Misc(int typeId, string code) =>
                (await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = typeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                })).Id;

            var prefixType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodePrefix", Description = "Prefix", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var seriesType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodeSeriesStatus", Description = "Series Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var allocType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodeAllocationStatus", Description = "Allocation Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;

            var prefixId = await Misc(prefixType, "CB");
            var seriesOpen = await Misc(seriesType, "Open");
            var seriesPartial = await Misc(seriesType, "PartiallyAllocated");
            var seriesFull = await Misc(seriesType, "FullyAllocated");
            var allocOpen = await Misc(allocType, "Open");

            async Task<PurchaseManagement.Domain.Entities.MiscMaster> Load(int id) =>
                await ctx.MiscMaster.AsNoTracking().FirstAsync(m => m.Id == id);

            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "Open")).ReturnsAsync(await Load(seriesOpen));
            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "PartiallyAllocated")).ReturnsAsync(await Load(seriesPartial));
            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "FullyAllocated")).ReturnsAsync(await Load(seriesFull));
            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeAllocationStatus", "Open")).ReturnsAsync(await Load(allocOpen));

            return new Seeded(prefixId, seriesOpen, seriesPartial, seriesFull);
        }

        private async Task<int> SeedSeriesAsync(ApplicationDbContext ctx, int prefixId, long start, long end)
        {
            var seriesRepo = new BarcodeSeriesCommandRepository(ctx, _fyMock.Object, _miscMock.Object);
            return await seriesRepo.CreateAsync(new PurchaseManagement.Domain.Entities.BarcodeSeries
            {
                PrefixId = prefixId, BarcodeStartNumber = start, BarcodeEndNumber = end,
                GenerationDate = GenDate, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            });
        }

        private static PurchaseManagement.Domain.Entities.BarcodeAllocation BuildAllocation(int seriesId, long from, long to) =>
            new()
            {
                AllocationDate = GenDate, EmployeeNo = "1023", EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = seriesId, BarcodeFrom = from, BarcodeTo = to,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Generate_AllocationNumber()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var seeded = await SeedMiscAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, seeded.PrefixId, 25000001, 25000500);

            var id = await CreateRepo(ctx).CreateAsync(BuildAllocation(seriesId, 25000001, 25000200));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BarcodeAllocation.FirstAsync(x => x.Id == id);
            saved.AllocationNumber.Should().Be("BBA-2025-0001");
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Parent_Series_To_PartiallyAllocated()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var seeded = await SeedMiscAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, seeded.PrefixId, 25000001, 25000500);

            await CreateRepo(ctx).CreateAsync(BuildAllocation(seriesId, 25000001, 25000200)); // 200 of 500
            ctx.ChangeTracker.Clear();

            var series = await ctx.BarcodeSeries.FirstAsync(x => x.Id == seriesId);
            series.AllocatedCount.Should().Be(200);
            series.StatusId.Should().Be(seeded.SeriesPartialId);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Parent_Series_To_FullyAllocated_When_WholeRangeAllocated()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var seeded = await SeedMiscAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, seeded.PrefixId, 25000001, 25000500);

            await CreateRepo(ctx).CreateAsync(BuildAllocation(seriesId, 25000001, 25000200));
            ctx.ChangeTracker.Clear();
            await CreateRepo(ctx).CreateAsync(BuildAllocation(seriesId, 25000201, 25000500)); // remaining 300
            ctx.ChangeTracker.Clear();

            var series = await ctx.BarcodeSeries.FirstAsync(x => x.Id == seriesId);
            series.AllocatedCount.Should().Be(500);
            series.StatusId.Should().Be(seeded.SeriesFullId);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Recompute_Parent_Series_Back_To_Open()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var seeded = await SeedMiscAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, seeded.PrefixId, 25000001, 25000500);

            var allocId = await CreateRepo(ctx).CreateAsync(BuildAllocation(seriesId, 25000001, 25000200));
            ctx.ChangeTracker.Clear();
            await CreateRepo(ctx).SoftDeleteAsync(allocId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var series = await ctx.BarcodeSeries.FirstAsync(x => x.Id == seriesId);
            series.AllocatedCount.Should().Be(0);
            series.StatusId.Should().Be(seeded.SeriesOpenId);
        }
    }
}
