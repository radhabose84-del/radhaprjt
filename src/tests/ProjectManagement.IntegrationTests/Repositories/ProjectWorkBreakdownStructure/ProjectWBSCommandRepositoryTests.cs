using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure;

namespace ProjectManagement.IntegrationTests.Repositories.ProjectWorkBreakdownStructure
{
    [Collection("DatabaseCollection")]
    public sealed class ProjectWBSCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectWBSCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static ProjectWorkBreakdownStructureCommandRepository CreateRepository(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedProjectAsync(ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var project = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = "PRJ001",
                ProjectName = "Test Project",
                ProjectTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                BudgetAmount = 100000m,
                BudgetYearId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddMonths(6),
                ProjectCategoryId = 1,
                AssetGroupId = 1,
                StatusId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ProjectMaster.Add(project);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return project.Id;
        }

        private static ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure BuildEntity(
            int projectId,
            string name = "Foundation Work") =>
            new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
            {
                ProjectId = projectId,
                WorkBreakdownStructureName = name,
                ResponsibleDepartmentId = 1,
                ResponsiblePerson = "John Doe",
                CurrencyId = 1,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                Level = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- ADD ---

        [Fact]
        public async Task AddAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var result = await CreateRepository(ctx).AddAsync(BuildEntity(projectId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var result = await CreateRepository(ctx).AddAsync(BuildEntity(projectId, "Civil Work"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProjectWorkBreakdownStructures
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.WorkBreakdownStructureName.Should().Be("Civil Work");
            saved.ProjectId.Should().Be(projectId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task AddAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var result = await CreateRepository(ctx).AddAsync(BuildEntity(projectId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ProjectWorkBreakdownStructures
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var created = await CreateRepository(ctx).AddAsync(BuildEntity(projectId, "Original WBS"));
            ctx.ChangeTracker.Clear();

            var toUpdate = await ctx.ProjectWorkBreakdownStructures
                .FirstAsync(x => x.Id == created.Id);
            toUpdate.WorkBreakdownStructureName = "Updated WBS";

            await CreateRepository(ctx).UpdateAsync(toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ProjectWorkBreakdownStructures
                .FirstOrDefaultAsync(x => x.Id == created.Id);
            updated!.WorkBreakdownStructureName.Should().Be("Updated WBS");
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var created = await CreateRepository(ctx).AddAsync(BuildEntity(projectId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(created.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var created = await CreateRepository(ctx).AddAsync(BuildEntity(projectId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ProjectWorkBreakdownStructures
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999);

            result.Should().BeFalse();
        }

        // --- NAME EXISTS ---

        [Fact]
        public async Task NameExistsAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            await CreateRepository(ctx).AddAsync(BuildEntity(projectId, "Foundation Work"));
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepository(ctx).NameExistsAsync(projectId, "Foundation Work");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task NameExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var exists = await CreateRepository(ctx).NameExistsAsync(projectId, "Nonexistent WBS");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsAsync_WithExcludeId_Should_Return_False_For_Self()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var created = await CreateRepository(ctx).AddAsync(BuildEntity(projectId, "Foundation Work"));
            ctx.ChangeTracker.Clear();

            var exists = await CreateRepository(ctx).NameExistsAsync(
                projectId, "Foundation Work", created.Id);

            exists.Should().BeFalse();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var created = await CreateRepository(ctx).AddAsync(BuildEntity(projectId, "Civil Work"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).GetByIdAsync(created.Id);

            result.Should().NotBeNull();
            result!.WorkBreakdownStructureName.Should().Be("Civil Work");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- GET PROJECT ---

        [Fact]
        public async Task GetProjectAsync_Should_Return_Project()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var projectId = await SeedProjectAsync(ctx);

            var result = await CreateRepository(ctx).GetProjectAsync(projectId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(projectId);
        }

        [Fact]
        public async Task GetProjectAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetProjectAsync(9999);

            result.Should().BeNull();
        }
    }
}
