using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
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
    public sealed class BarcodeSeriesCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IFinancialYearLookup> _fyMock = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscMock = new(MockBehavior.Loose);

        private static readonly DateTimeOffset GenDate = new(2025, 6, 3, 0, 0, 0, TimeSpan.Zero);

        public BarcodeSeriesCommandRepositoryTests(DbFixture fixture)
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

        private BarcodeSeriesCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _fyMock.Object, _miscMock.Object);

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

        private static PurchaseManagement.Domain.Entities.BarcodeSeries BuildEntity(int prefixId, long start = 25000001, long end = 25005000) =>
            new()
            {
                PrefixId = prefixId,
                BarcodeStartNumber = start,
                BarcodeEndNumber = end,
                GenerationDate = GenDate,
                Remarks = "Range A",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Generate_SeriesNumber_FromFinancialYear()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BarcodeSeries.FirstAsync(x => x.Id == id);
            saved.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_To_Open()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, statusId) = await SeedPrerequisitesAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BarcodeSeries.FirstAsync(x => x.Id == id);
            saved.StatusId.Should().Be(statusId);
        }

        [Fact]
        public async Task CreateAsync_Should_Increment_SeriesNumber_PerFinancialYear()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);

            await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId, 25000001, 25005000));
            ctx.ChangeTracker.Clear();
            var secondId = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId, 25005001, 25010000));
            ctx.ChangeTracker.Clear();

            var second = await ctx.BarcodeSeries.FirstAsync(x => x.Id == secondId);
            second.BarcodeSeriesNumber.Should().Be("BCS-2025-0002");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new PurchaseManagement.Domain.Entities.BarcodeSeries
            {
                Id = id,
                PrefixId = prefixId,
                BarcodeStartNumber = 25000001,
                BarcodeEndNumber = 25009999,
                Remarks = "Updated",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BarcodeSeries.FirstAsync(x => x.Id == id);
            updated.BarcodeEndNumber.Should().Be(25009999);
            updated.Remarks.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_SeriesNumber()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new PurchaseManagement.Domain.Entities.BarcodeSeries
            {
                Id = id,
                PrefixId = prefixId,
                BarcodeStartNumber = 25000001,
                BarcodeEndNumber = 25006000,
                Remarks = "X",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BarcodeSeries.FirstAsync(x => x.Id == id);
            updated.BarcodeSeriesNumber.Should().Be("BCS-2025-0001");
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (prefixId, _) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(prefixId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.BarcodeSeries.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
