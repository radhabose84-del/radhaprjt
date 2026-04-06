using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ShiftMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ShiftMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ShiftMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ShiftMasterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.ShiftMaster BuildEntity(
            string code = "SHF001",
            string name = "Morning Shift") =>
            new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                ShiftCode = code,
                ShiftName = name,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[ShiftMasterDetails]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[ShiftMaster]");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF002", "Night Shift"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ShiftMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ShiftCode.Should().Be("SHF002");
            saved.ShiftName.Should().Be("Night Shift");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF003"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ShiftMaster.FirstOrDefaultAsync(x => x.Id == newId);

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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF004", "Original Shift"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                Id = newId,
                ShiftCode = "SHF004",
                ShiftName = "Updated Shift",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF005", "Before Update Shift"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                Id = newId,
                ShiftCode = "SHF005",
                ShiftName = "After Update Shift",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ShiftMaster.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.ShiftName.Should().Be("After Update Shift");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                Id = 9999,
                ShiftCode = "SHF999",
                ShiftName = "No Such Shift",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeFalse();
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF006"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("SHF007"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ShiftMaster
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
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
