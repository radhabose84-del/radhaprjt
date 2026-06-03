using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
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
    public sealed class BarcodeAllocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IFinancialYearLookup> _fyMock = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscMock = new(MockBehavior.Loose);

        private static readonly DateTimeOffset GenDate = new(2025, 6, 3, 0, 0, 0, TimeSpan.Zero);

        public BarcodeAllocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _fyMock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new() { FinancialYearId = 1, StartYear = "2025", StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2026, 3, 31), IsActive = true }
            });
        }

        private BarcodeAllocationQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fyMock.Object, _fixture.IpMock.Object);

        private async Task<int> SeedMiscPrefixAndSeriesStatusesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var miscRepo = new MiscMasterCommandRepository(ctx);

            async Task<int> Misc(int typeId, string code) =>
                (await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                { MiscTypeId = typeId, Code = code, Description = code, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;

            var prefixType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodePrefix", Description = "Prefix", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var seriesType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodeSeriesStatus", Description = "Series Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var allocType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = "BarcodeAllocationStatus", Description = "Allocation Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;

            var prefixId = await Misc(prefixType, "CB");
            var seriesOpen = await Misc(seriesType, "Open");
            var seriesPartial = await Misc(seriesType, "PartiallyAllocated");
            await Misc(seriesType, "FullyAllocated");
            var allocOpen = await Misc(allocType, "Open");

            async Task<PurchaseManagement.Domain.Entities.MiscMaster> Load(int id) =>
                await ctx.MiscMaster.AsNoTracking().FirstAsync(m => m.Id == id);

            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "Open")).ReturnsAsync(await Load(seriesOpen));
            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "PartiallyAllocated")).ReturnsAsync(await Load(seriesPartial));
            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeAllocationStatus", "Open")).ReturnsAsync(await Load(allocOpen));

            return prefixId;
        }

        private async Task<int> SeedSeriesAsync(ApplicationDbContext ctx, int prefixId, long start, long end)
        {
            var seriesRepo = new BarcodeSeriesCommandRepository(ctx, _fyMock.Object, _miscMock.Object);
            return await seriesRepo.CreateAsync(new PurchaseManagement.Domain.Entities.BarcodeSeries
            { PrefixId = prefixId, BarcodeStartNumber = start, BarcodeEndNumber = end, GenerationDate = GenDate, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
        }

        private async Task<int> SeedAllocationAsync(ApplicationDbContext ctx, int seriesId, long from, long to)
        {
            var repo = new BarcodeAllocationCommandRepository(ctx, _fyMock.Object, _miscMock.Object);
            return await repo.CreateAsync(new PurchaseManagement.Domain.Entities.BarcodeAllocation
            {
                AllocationDate = GenDate, EmployeeNo = "1023", EmployeeName = "Rajesh Kumar",
                BarcodeSeriesId = seriesId, BarcodeFrom = from, BarcodeTo = to,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            });
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Record_With_Computed_Columns()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);
            await SeedAllocationAsync(ctx, seriesId, 25000001, 25000200);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items[0].EmployeeName.Should().Be("Rajesh Kumar");
            items[0].Prefix.Should().Be("CB");
            items[0].Status.Should().Be("Open");
            items[0].TotalAllocatedQuantity.Should().Be(200);
            items[0].BalanceQuantity.Should().Be(200);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);
            var allocId = await SeedAllocationAsync(ctx, seriesId, 25000001, 25000200);

            var dto = await CreateQueryRepo().GetByIdAsync(allocId);

            dto.Should().NotBeNull();
            dto!.AllocationNumber.Should().Be("BBA-2025-0001");
            dto.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
        }

        [Fact]
        public async Task RangeOverlapsInSeriesAsync_Should_Detect_Overlap_Within_Same_Series()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);
            await SeedAllocationAsync(ctx, seriesId, 25000001, 25000200);

            (await CreateQueryRepo().RangeOverlapsInSeriesAsync(seriesId, 25000150, 25000300)).Should().BeTrue();
            (await CreateQueryRepo().RangeOverlapsInSeriesAsync(seriesId, 25000201, 25000400)).Should().BeFalse();
        }

        [Fact]
        public async Task IsWithinSeriesRangeAsync_Should_Validate_Bounds()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);

            (await CreateQueryRepo().IsWithinSeriesRangeAsync(seriesId, 25000001, 25000500)).Should().BeTrue();
            (await CreateQueryRepo().IsWithinSeriesRangeAsync(seriesId, 25000001, 25000600)).Should().BeFalse();
        }

        [Fact]
        public async Task GetAvailableSeriesAsync_Should_Return_Series_With_Balance()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);
            await SeedAllocationAsync(ctx, seriesId, 25000001, 25000200);

            var list = await CreateQueryRepo().GetAvailableSeriesAsync(null);

            list.Should().ContainSingle(s => s.Id == seriesId && s.BalanceCount == 300);
        }

        [Fact]
        public async Task GetMaxAllocatedToForSeriesAsync_Should_Return_StartMinusOne_When_None()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);

            (await CreateQueryRepo().GetMaxAllocatedToForSeriesAsync(seriesId)).Should().Be(25000000);
        }

        [Fact]
        public async Task GetMaxAllocatedToForSeriesAsync_Should_Return_HighestAllocatedTo()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var prefixId = await SeedMiscPrefixAndSeriesStatusesAsync(ctx);
            var seriesId = await SeedSeriesAsync(ctx, prefixId, 25000001, 25000500);
            await SeedAllocationAsync(ctx, seriesId, 25000001, 25000200);

            (await CreateQueryRepo().GetMaxAllocatedToForSeriesAsync(seriesId)).Should().Be(25000200);
        }
    }
}
