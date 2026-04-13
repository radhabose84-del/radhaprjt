using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepository(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "MAINT_T_MC")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private static MaintenanceManagement.Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "MM_CMD001",
            string description = "Test Misc Master") =>
            new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM_STATUS", "Open Status"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("MM_STATUS");
            saved.Description.Should().Be("Open Status");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

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
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_U1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM_UPD001", "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(entity.Id, new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                Id = entity.Id,
                MiscTypeId = miscTypeId,
                Code = "MM_UPD001",
                Description = "Updated Description",
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_U2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM_UPD002", "Original Desc"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(entity.Id, new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                Id = entity.Id,
                MiscTypeId = miscTypeId,
                Code = "MM_UPD002",
                Description = "Updated Desc",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == entity.Id);
            updated!.Description.Should().Be("Updated Desc");
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_D1");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_T_D2");

            var entity = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM_DEL001"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(entity.Id,
                new MaintenanceManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
