using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;

namespace MaintenanceManagement.IntegrationTests.Repositories.Feeder
{
    [Collection("DatabaseCollection")]
    public sealed class FeederCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private FeederCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private static MaintenanceManagement.Domain.Entities.Power.Feeder BuildEntity(
            string code = "FDR_CMD001",
            string name = "Test Feeder",
            int unitId = 1,
            int feederGroupId = 1,
            int feederTypeId = 1,
            int departmentId = 1) =>
            new()
            {
                FeederCode = code,
                FeederName = name,
                UnitId = unitId,
                FeederGroupId = feederGroupId,
                FeederTypeId = feederTypeId,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                MeterAvailable = false,
                HighPriority = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task SeedFeederGroupAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            if (!await ctx.FeederGroup.AnyAsync(x => x.Id == 1))
            {
                await ctx.FeederGroup.AddAsync(new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = "FG001",
                    FeederGroupName = "Test Group",
                    UnitId = 1,
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
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[Feeder]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[FeederGroup]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR001", "Main Feeder"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Feeder.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.FeederCode.Should().Be("FDR001");
            saved.FeederName.Should().Be("Main Feeder");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR002"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Feeder.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR003", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.Power.Feeder
            {
                FeederCode = "FDR003",
                FeederName = "Updated Feeder",
                UnitId = 1,
                FeederGroupId = 1,
                FeederTypeId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR004", "Before Update"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.Power.Feeder
            {
                FeederCode = "FDR004",
                FeederName = "After Update",
                UnitId = 1,
                FeederGroupId = 1,
                FeederTypeId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.Feeder.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.FeederName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new MaintenanceManagement.Domain.Entities.Power.Feeder
            {
                FeederName = "Ghost",
                EffectiveDate = DateTimeOffset.UtcNow,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR005"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await SeedFeederGroupAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("FDR006"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Feeder
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
