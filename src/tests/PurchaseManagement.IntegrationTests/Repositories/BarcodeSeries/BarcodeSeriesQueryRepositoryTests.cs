using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.BarcodeSeries;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.BarcodeSeries
{
    [Collection("DatabaseCollection")]
    public sealed class BarcodeSeriesQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IFinancialYearLookup> _fyMock = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscMock = new(MockBehavior.Loose);

        private static readonly DateTimeOffset GenDate = new(2025, 6, 3, 0, 0, 0, TimeSpan.Zero);

        public BarcodeSeriesQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _fyMock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new()
                {
                    FinancialYearId = 1,
                    FinancialYearName = "2025-26",
                    StartYear = "2025",
                    StartDate = new DateTime(2025, 4, 1),
                    EndDate = new DateTime(2026, 3, 31),
                    IsActive = true
                }
            });
        }

        private BarcodeSeriesQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fyMock.Object);

        private BarcodeSeriesCommandRepository CreateCommandRepo(ApplicationDbContext ctx) =>
            new(ctx, _fyMock.Object, _miscMock.Object);

        private async Task<int> SeedSeriesAsync(ApplicationDbContext ctx, int prefixId, long start, long end)
        {
            var entity = new PurchaseManagement.Domain.Entities.BarcodeSeries
            {
                PrefixId = prefixId,
                BarcodeStartNumber = start,
                BarcodeEndNumber = end,
                GenerationDate = GenDate,
                Remarks = "Range",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        private async Task<(int prefixId, int statusId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var prefixType = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BarcodePrefix",
                Description = "Bale Barcode Prefix",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            var statusType = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BarcodeSeriesStatus",
                Description = "Bale Barcode Series Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var prefix = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = prefixType.Id,
                Code = "CB",
                Description = "CB",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            var status = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = statusType.Id,
                Code = "Open",
                Description = "Open",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            _miscMock.Setup(m => m.GetMiscMasterByName("BarcodeSeriesStatus", "Open")).ReturnsAsync(status);

            return (prefix.Id, status.Id);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record_With_Computed_Columns()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].Prefix.Should().Be("CB");
            items[0].Status.Should().Be("Open");
            items[0].TotalBarcodeCount.Should().Be(5000);
            items[0].Balance.Should().Be(5000);
            items[0].BarcodeFormatPreview.Should().Be("CB25000001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
            dto.TotalBarcodeCount.Should().Be(5000);
        }

        [Fact]
        public async Task RangeOverlapsAsync_Should_Return_True_For_Overlap()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);

            var overlaps = await CreateQueryRepo().RangeOverlapsAsync(25004000, 25006000);

            overlaps.Should().BeTrue();
        }

        [Fact]
        public async Task RangeOverlapsAsync_Should_Return_False_For_NonOverlap()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);

            var overlaps = await CreateQueryRepo().RangeOverlapsAsync(25005001, 25010000);

            overlaps.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidPrefixAsync_Should_Return_True_For_Seeded_Prefix()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);

            var valid = await CreateQueryRepo().IsValidPrefixAsync(prefixId);

            valid.Should().BeTrue();
        }

        [Fact]
        public async Task GetMaxEndNumberAsync_Should_Return_Highest_End()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);
            await SeedSeriesAsync(ctx, prefixId, 25005001, 25010000);

            var max = await CreateQueryRepo().GetMaxEndNumberAsync();

            max.Should().Be(25010000);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedSeriesAsync(ctx, prefixId, 25000001, 25005000);
            ctx.ChangeTracker.Clear();
            var entity = await ctx.BarcodeSeries.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("BCS", CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
