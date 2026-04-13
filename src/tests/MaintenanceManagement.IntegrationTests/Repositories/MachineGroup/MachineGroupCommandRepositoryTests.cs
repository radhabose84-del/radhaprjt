using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineGroup
{
    [Collection("DatabaseCollection")]
    public sealed class MachineGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineGroupCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.MachineGroup BuildEntity(
            string groupName = "Test Machine Group",
            int unitId = 1,
            int departmentId = 1) =>
            new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = groupName,
                Manufacturer = 1,
                UnitId = unitId,
                DepartmentId = departmentId,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity("Drilling Group", 1, 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineGroup.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.GroupName.Should().Be("Drilling Group");
            saved.UnitId.Should().Be(1);
            saved.DepartmentId.Should().Be(2);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MachineGroup.FirstOrDefaultAsync(x => x.Id == result.Id);

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

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity("Original Group"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(entity.Id, new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                Id = entity.Id,
                GroupName = "Updated Group",
                Manufacturer = 2,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active,
                PowerSource = true
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity("Before Update"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(entity.Id, new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                Id = entity.Id,
                GroupName = "After Update",
                Manufacturer = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active,
                PowerSource = false
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MachineGroup.FirstOrDefaultAsync(x => x.Id == entity.Id);
            updated!.GroupName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                Id = 9999,
                GroupName = "Non Existent",
                Manufacturer = 1,
                DepartmentId = 1,
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

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.MachineGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity("To Delete Group"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.MachineGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MachineGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.MachineGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
