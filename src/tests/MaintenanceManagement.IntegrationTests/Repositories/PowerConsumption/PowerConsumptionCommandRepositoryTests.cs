using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;
using MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption;

namespace MaintenanceManagement.IntegrationTests.Repositories.PowerConsumption
{
    [Collection("DatabaseCollection")]
    public sealed class PowerConsumptionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PowerConsumptionCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PowerConsumptionCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscMasterAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscTypeResult = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"PC_MT_{suffix}",
                    Description = "PC Test Type",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var miscResult = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeResult.Id,
                    Code = $"PC_MM_{suffix}",
                    Description = $"Feeder Type {suffix}",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return miscResult.Id;
        }

        private async Task<int> SeedFeederGroupAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = $"PC_FG_{suffix}",
                    FeederGroupName = $"Feeder Group {suffix}",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedFeederAsync(string suffix, int feederGroupId, int feederTypeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.Feeder
                {
                    FeederCode = $"PC_F_{suffix}",
                    FeederName = $"Feeder {suffix}",
                    FeederGroupId = feederGroupId,
                    FeederTypeId = feederTypeId,
                    DepartmentId = 1,
                    Description = "Desc",
                    MultiplicationFactor = 1.0m,
                    EffectiveDate = DateTimeOffset.UtcNow,
                    OpeningReading = 0m,
                    HighPriority = false,
                    Target = 0m,
                    UnitId = 1,
                    MeterAvailable = false,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private static MaintenanceManagement.Domain.Entities.Power.PowerConsumption BuildEntity(
            int feederTypeId, int feederId, decimal opening = 100m, decimal closing = 200m) =>
            new MaintenanceManagement.Domain.Entities.Power.PowerConsumption
            {
                FeederTypeId = feederTypeId,
                FeederId = feederId,
                UnitId = 1,
                OpeningReading = opening,
                ClosingReading = closing,
                TotalUnits = closing - opening,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var feederTypeId = await SeedMiscMasterAsync("C1");
            var feederGroupId = await SeedFeederGroupAsync("C1");
            var feederId = await SeedFeederAsync("C1", feederGroupId, feederTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(feederTypeId, feederId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var feederTypeId = await SeedMiscMasterAsync("C2");
            var feederGroupId = await SeedFeederGroupAsync("C2");
            var feederId = await SeedFeederAsync("C2", feederGroupId, feederTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(feederTypeId, feederId, opening: 500m, closing: 750m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PowerConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.FeederTypeId.Should().Be(feederTypeId);
            saved.FeederId.Should().Be(feederId);
            saved.UnitId.Should().Be(1);
            saved.OpeningReading.Should().Be(500m);
            saved.ClosingReading.Should().Be(750m);
            saved.TotalUnits.Should().Be(250m);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var feederTypeId = await SeedMiscMasterAsync("C3");
            var feederGroupId = await SeedFeederGroupAsync("C3");
            var feederId = await SeedFeederAsync("C3", feederGroupId, feederTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(feederTypeId, feederId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PowerConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Allow_Multiple_Records_For_Same_Feeder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var feederTypeId = await SeedMiscMasterAsync("C4");
            var feederGroupId = await SeedFeederGroupAsync("C4");
            var feederId = await SeedFeederAsync("C4", feederGroupId, feederTypeId);

            var id1 = await CreateRepository(ctx).CreateAsync(
                BuildEntity(feederTypeId, feederId, opening: 100m, closing: 200m));
            var id2 = await CreateRepository(ctx).CreateAsync(
                BuildEntity(feederTypeId, feederId, opening: 200m, closing: 350m));

            id1.Should().BeGreaterThan(0);
            id2.Should().BeGreaterThan(0);
            id2.Should().NotBe(id1);
        }
    }
}
