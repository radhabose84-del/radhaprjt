using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.CostCenter
{
    [Collection("DatabaseCollection")]
    public sealed class CostCenterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CostCenterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CostCenterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static MaintenanceManagement.Domain.Entities.CostCenter BuildEntity(
            string code = "CC_CMD001",
            string name = "Test Cost Center",
            int unitId = 1,
            int departmentId = 1) =>
            new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = code,
                CostCenterName = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                BudgetAllocated = 10000,
                Remarks = "Test Remarks",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Maintenance].[CostCenter]");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC001", "Finance Center", 1, 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CostCenter.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CostCenterCode.Should().Be("CC001");
            saved.CostCenterName.Should().Be("Finance Center");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC002"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CostCenter.FirstOrDefaultAsync(x => x.Id == newId);

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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC003", "Original Name"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.CostCenter
            {
                Id = newId,
                CostCenterName = "Updated Name",
                UnitId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = BaseEntity.Status.Active
            });

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC004", "Before Update"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new MaintenanceManagement.Domain.Entities.CostCenter
            {
                Id = newId,
                CostCenterName = "After Update",
                UnitId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.CostCenter.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.CostCenterName.Should().Be("After Update");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_MinusOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new MaintenanceManagement.Domain.Entities.CostCenter
            {
                Id = 9999,
                CostCenterName = "No Such Center",
                UnitId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC005"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.CostCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("CC006"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new MaintenanceManagement.Domain.Entities.CostCenter { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.CostCenter
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
                new MaintenanceManagement.Domain.Entities.CostCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(-1);
        }
    }
}
