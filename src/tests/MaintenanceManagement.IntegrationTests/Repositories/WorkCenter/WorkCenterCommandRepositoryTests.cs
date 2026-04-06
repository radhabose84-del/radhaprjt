using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.WorkCenter
{
    [Collection("DatabaseCollection")]
    public sealed class WorkCenterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkCenterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkCenterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.WorkCenter BuildEntity(
            string code = "WC_CMD001",
            string name = "Test Work Center",
            int unitId = 1,
            int departmentId = 1) =>
            new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                WorkCenterCode = code,
                WorkCenterName = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[WorkCenter]");

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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC001", "Assembly Line", 1, 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WorkCenter.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.WorkCenterCode.Should().Be("WC001");
            saved.WorkCenterName.Should().Be("Assembly Line");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC002"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.WorkCenter.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC003", "Original WC"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                Id = newId,
                WorkCenterName = "Updated WC",
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC004", "Before Update WC"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                Id = newId,
                WorkCenterName = "After Update WC",
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.WorkCenter.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.WorkCenterName.Should().Be("After Update WC");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_MinusOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                Id = 9999,
                WorkCenterName = "No Such WC",
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(-1);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC005"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.WorkCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("WC006"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.WorkCenter { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.WorkCenter
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_MinusOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new MaintenanceManagement.Domain.Entities.WorkCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }
    }
}
