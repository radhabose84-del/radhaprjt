using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.MiscMaster;
using ProjectManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace ProjectManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static MiscMasterCommandRepository CreateRepository(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private static MiscTypeMasterCommandRepository CreateMiscTypeRepo(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeAsync(ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx,
            string code = "PROJ_STATUS")
        {
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Project Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await CreateMiscTypeRepo(ctx).CreateAsync(entity);
            return result.Id;
        }

        private static ProjectManagement.Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "OPEN",
            string description = "Open") =>
            new ProjectManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Project].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Project].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "CLOSED", "Closed"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("CLOSED");
            saved.Description.Should().Be("Closed");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_AutoAssign_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            result.SortOrder.Should().BeGreaterThanOrEqualTo(0);
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "OPEN", "Open"));
            ctx.ChangeTracker.Clear();

            created.Description = "Updated Description";
            var result = await CreateRepository(ctx).UpdateAsync(created.Id, created);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "OPEN", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new ProjectManagement.Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "OPEN",
                Description = "Modified Description",
                SortOrder = created.SortOrder,
                IsActive = BaseEntity.Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated!.Description.Should().Be("Modified Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = new ProjectManagement.Domain.Entities.MiscMaster
            {
                Id = 9999,
                MiscTypeId = 1,
                Code = "NOTEXIST",
                Description = "Not Found",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, entity);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var toDelete = new ProjectManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var toDelete = new ProjectManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var toDelete = new ProjectManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, toDelete);

            result.Should().BeFalse();
        }
    }
}
