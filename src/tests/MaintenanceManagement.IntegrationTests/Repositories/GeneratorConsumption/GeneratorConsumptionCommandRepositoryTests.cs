using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.GeneratorConsumption;

namespace MaintenanceManagement.IntegrationTests.Repositories.GeneratorConsumption
{
    [Collection("DatabaseCollection")]
    public sealed class GeneratorConsumptionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GeneratorConsumptionCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GeneratorConsumptionCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private static MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption BuildEntity(
            int generatorId = 1,
            decimal openingReading = 100m,
            decimal closingReading = 200m) =>
            new()
            {
                GeneratorId = generatorId,
                StartTime = DateTimeOffset.UtcNow.AddHours(-2),
                EndTime = DateTimeOffset.UtcNow,
                DieselConsumption = 10m,
                OpeningEnergyReading = openingReading,
                ClosingEnergyReading = closingReading,
                Energy = closingReading - openingReading,
                RunningHours = 2m,
                UnitId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[GeneratorConsumption]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(openingReading: 50m, closingReading: 150m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.GeneratorConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.OpeningEnergyReading.Should().Be(50m);
            saved.ClosingEnergyReading.Should().Be(150m);
            saved.Energy.Should().Be(100m);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.GeneratorConsumption.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }
    }
}
