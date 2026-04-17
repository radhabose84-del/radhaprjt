using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.DutyMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.DutyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class DutyMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DutyMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DutyMasterCommandRepository CreateRepository(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static PurchaseManagement.Domain.Entities.DutyMaster BuildEntity(
            int dutyCategoryId,
            int countryOfOriginApplicability,
            string dutyCode = "DUT-001",
            string tariffNumber = "84.71") =>
            new()
            {
                DutyCode = dutyCode,
                TariffNumber = tariffNumber,
                DutyCategoryId = dutyCategoryId,
                CountryOfOriginApplicability = countryOfOriginApplicability,
                BasicCustomsDutyPercentage = 10m,
                SocialWelfareSurchargePercentage = 1m,
                IGSTPercentage = 18m,
                EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-30),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<(int miscTypeId, int miscId)> SeedPrerequisitesAsync(
            PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscTypeRepo = new MiscTypeMasterCommandRepository(ctx);
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "MT001",
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var mt = await miscTypeRepo.CreateAsync(miscType);
            var mtId = mt.Id;

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var misc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "MSC001",
                Description = "Test Misc",
                MiscTypeId = mtId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var m = await miscRepo.CreateAsync(misc);
            return (mtId, m.Id);
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(miscId, miscId);
            var id = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);

            var entity = BuildEntity(miscId, miscId, "DUT-002", "85.17");
            var id = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.DutyMaster>()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.DutyCode.Should().Be("DUT-002");
            saved.TariffNumber.Should().Be("85.17");
            saved.BasicCustomsDutyPercentage.Should().Be(10m);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, miscId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, miscId) = await SeedPrerequisitesAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, miscId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<PurchaseManagement.Domain.Entities.DutyMaster>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
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
