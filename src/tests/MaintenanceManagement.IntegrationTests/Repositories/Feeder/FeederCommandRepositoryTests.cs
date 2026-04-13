using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.Feeder
{
    [Collection("DatabaseCollection")]
    public sealed class FeederCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FeederCommandRepository CreateRepo(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedFeederGroupAsync(string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = code,
                    FeederGroupName = name,
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedFeederTypeAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"FDR_MT_{suffix}",
                    Description = "FeederType",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id,
                    Code = $"FDR_MM_{suffix}",
                    Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return misc.Id;
        }

        private MaintenanceManagement.Domain.Entities.Power.Feeder BuildEntity(
            string code, string name, int feederGroupId, int feederTypeId) =>
            new MaintenanceManagement.Domain.Entities.Power.Feeder
            {
                FeederCode = code,
                FeederName = name,
                FeederGroupId = feederGroupId,
                FeederTypeId = feederTypeId,
                DepartmentId = 1,
                Description = "Test feeder",
                MultiplicationFactor = 1.0m,
                EffectiveDate = DateTimeOffset.UtcNow,
                OpeningReading = 0.0m,
                HighPriority = false,
                Target = 0.0m,
                UnitId = 1,
                MeterAvailable = true,
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
            var fgId = await SeedFeederGroupAsync("FG_C1", "Group C1");
            var ftId = await SeedFeederTypeAsync("C1");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR001", "Feeder 1", fgId, ftId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_C2", "Group C2");
            var ftId = await SeedFeederTypeAsync("C2");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR002", "Feeder 2", fgId, ftId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Feeder.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.FeederCode.Should().Be("FDR002");
            saved.FeederName.Should().Be("Feeder 2");
            saved.FeederGroupId.Should().Be(fgId);
            saved.FeederTypeId.Should().Be(ftId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_C3", "Group C3");
            var ftId = await SeedFeederTypeAsync("C3");

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR003", "Feeder 3", fgId, ftId));
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
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_U1", "Group U1");
            var ftId = await SeedFeederTypeAsync("U1");
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR_U1", "Original", fgId, ftId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(newId, BuildEntity("FDR_U1", "Updated Name", fgId, ftId));

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_U2", "Group U2");
            var ftId = await SeedFeederTypeAsync("U2");
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR_U2", "Before", fgId, ftId));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(newId, BuildEntity("FDR_U2", "After Update", fgId, ftId));
            ctx.ChangeTracker.Clear();

            var updated = await ctx.Feeder.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.FeederName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_U3", "Group U3");
            var ftId = await SeedFeederTypeAsync("U3");

            var result = await CreateRepo(ctx).UpdateAsync(99999, BuildEntity("NOEX", "No Such", fgId, ftId));

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_D1", "Group D1");
            var ftId = await SeedFeederTypeAsync("D1");
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR_D1", "Delete Me", fgId, ftId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var fgId = await SeedFeederGroupAsync("FG_D2", "Group D2");
            var ftId = await SeedFeederTypeAsync("D2");
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("FDR_D2", "Soft Delete", fgId, ftId));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteAsync(newId,
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
            await ClearTablesAsync(ctx);

            var result = await CreateRepo(ctx).DeleteAsync(99999,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
