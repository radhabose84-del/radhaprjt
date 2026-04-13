using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMasterDetailRepo;

namespace MaintenanceManagement.IntegrationTests.Repositories.ShiftMasterDetail
{
    [Collection("DatabaseCollection")]
    public sealed class ShiftMasterDetailCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ShiftMasterDetailCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ShiftMasterDetailCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedShiftMasterAsync(string code = "SMD_CMD_SM1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ShiftMasterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                ShiftCode = code,
                ShiftName = $"Shift {code}",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static MaintenanceManagement.Domain.Entities.ShiftMasterDetail BuildEntity(
            int shiftMasterId,
            int unitId = 1) =>
            new MaintenanceManagement.Domain.Entities.ShiftMasterDetail
            {
                ShiftMasterId = shiftMasterId,
                UnitId = unitId,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(16, 0),
                DurationInHours = 8,
                BreakDurationInMinutes = 30,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                ShiftSupervisorId = 1,
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
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId, 1));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ShiftMasterDetail.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ShiftMasterId.Should().Be(shiftMasterId);
            saved.UnitId.Should().Be(1);
            saved.DurationInHours.Should().Be(8);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S3");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ShiftMasterDetail.FirstOrDefaultAsync(x => x.Id == newId);

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
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S4");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail
            {
                Id = newId,
                ShiftMasterId = shiftMasterId,
                UnitId = 1,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                DurationInHours = 8,
                BreakDurationInMinutes = 45,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                ShiftSupervisorId = 2,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S5");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail
            {
                Id = newId,
                ShiftMasterId = shiftMasterId,
                UnitId = 1,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(18, 0),
                DurationInHours = 8,
                BreakDurationInMinutes = 60,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                ShiftSupervisorId = 5,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ShiftMasterDetail.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.ShiftSupervisorId.Should().Be(5);
            updated.BreakDurationInMinutes.Should().Be(60);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail
            {
                Id = 9999,
                ShiftMasterId = 1,
                UnitId = 1,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(16, 0),
                DurationInHours = 8,
                BreakDurationInMinutes = 30,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                ShiftSupervisorId = 1,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeFalse();
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S6");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var shiftMasterId = await SeedShiftMasterAsync("SMD_CMD_S7");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(shiftMasterId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ShiftMasterDetail
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
