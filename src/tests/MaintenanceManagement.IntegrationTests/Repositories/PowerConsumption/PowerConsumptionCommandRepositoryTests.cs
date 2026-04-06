using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption;

namespace MaintenanceManagement.IntegrationTests.Repositories.PowerConsumption
{
    [Collection("DatabaseCollection")]
    public sealed class PowerConsumptionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PowerConsumptionCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PowerConsumptionCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private static MaintenanceManagement.Domain.Entities.Power.PowerConsumption BuildEntity(
            int feederId = 1,
            int feederTypeId = 1,
            decimal openingReading = 10m,
            decimal closingReading = 20m) =>
            new()
            {
                FeederId = feederId,
                FeederTypeId = feederTypeId,
                UnitId = 1,
                OpeningReading = openingReading,
                ClosingReading = closingReading,
                TotalUnits = (closingReading - openingReading) * 1000m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task SeedFeederPrerequisitesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            if (!await ctx.FeederGroup.AnyAsync(x => x.Id == 1))
            {
                await ctx.FeederGroup.AddAsync(new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = "FGP001",
                    FeederGroupName = "Power Test Group",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            if (!await ctx.Feeder.AnyAsync(x => x.Id == 1))
            {
                await ctx.Feeder.AddAsync(new MaintenanceManagement.Domain.Entities.Power.Feeder
                {
                    FeederCode = "FDR_PWR001",
                    FeederName = "Power Feeder",
                    UnitId = 1,
                    FeederGroupId = ctx.FeederGroup.First().Id,
                    FeederTypeId = 1,
                    DepartmentId = 1,
                    EffectiveDate = DateTimeOffset.UtcNow,
                    MeterAvailable = false,
                    HighPriority = false,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }
        }

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[PowerConsumption]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederPrerequisitesAsync(ctx);

            var feederId = ctx.Feeder.First().Id;
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(feederId: feederId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederPrerequisitesAsync(ctx);

            var feederId = ctx.Feeder.First().Id;
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(feederId: feederId, openingReading: 5m, closingReading: 15m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PowerConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.OpeningReading.Should().Be(5m);
            saved.ClosingReading.Should().Be(15m);
            saved.TotalUnits.Should().Be(10000m); // (15 - 5) * 1000
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederPrerequisitesAsync(ctx);

            var feederId = ctx.Feeder.First().Id;
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(feederId: feederId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PowerConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }
    }
}
