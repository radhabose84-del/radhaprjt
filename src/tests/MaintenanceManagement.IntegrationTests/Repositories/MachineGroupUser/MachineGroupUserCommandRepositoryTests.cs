using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroupUser;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineGroupUser
{
    [Collection("DatabaseCollection")]
    public sealed class MachineGroupUserCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineGroupUserCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineGroupUserCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMachineGroupAsync(string groupName = "MGU_CMD_Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MachineGroupCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = groupName,
                Manufacturer = 1,
                UnitId = 1,
                DepartmentId = 1,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private static MaintenanceManagement.Domain.Entities.MachineGroupUser BuildEntity(
            int machineGroupId,
            int departmentId = 1,
            int userId = 100) =>
            new MaintenanceManagement.Domain.Entities.MachineGroupUser
            {
                MachineGroupId = machineGroupId,
                DepartmentId = departmentId,
                UserId = userId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineGroupUser]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineGroup]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G1");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G2");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId, 2, 200));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineGroupUser.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MachineGroupId.Should().Be(machineGroupId);
            saved.DepartmentId.Should().Be(2);
            saved.UserId.Should().Be(200);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G3");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineGroupUser.FirstOrDefaultAsync(x => x.Id == newId);

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
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G4");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId, 1, 100));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.MachineGroupUser
            {
                Id = newId,
                MachineGroupId = machineGroupId,
                DepartmentId = 2,
                UserId = 200,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G5");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId, 1, 100));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new MaintenanceManagement.Domain.Entities.MachineGroupUser
            {
                Id = newId,
                MachineGroupId = machineGroupId,
                DepartmentId = 3,
                UserId = 300,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MachineGroupUser.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.UserId.Should().Be(300);
            updated.DepartmentId.Should().Be(3);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G6");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineGroupUser { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var machineGroupId = await SeedMachineGroupAsync("MGU_CMD_G7");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(machineGroupId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.MachineGroupUser { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MachineGroupUser
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
                new MaintenanceManagement.Domain.Entities.MachineGroupUser { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
