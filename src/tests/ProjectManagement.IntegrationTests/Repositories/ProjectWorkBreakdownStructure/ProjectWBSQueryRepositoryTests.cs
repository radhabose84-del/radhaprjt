using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure;

namespace ProjectManagement.IntegrationTests.Repositories.ProjectWorkBreakdownStructure
{
    [Collection("DatabaseCollection")]
    public sealed class ProjectWBSQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectWBSQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ProjectWorkBreakdownStructureQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            return new ProjectWorkBreakdownStructureQueryRepository(conn, ipMock.Object);
        }

        private static ProjectWorkBreakdownStructureCommandRepository CreateCommandRepo(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedProjectAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
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
            return project.Id;
        }

        private async Task<int> SeedWbsAsync(int projectId, string name = "Foundation Work")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
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
            var result = await CreateCommandRepo(ctx).AddAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Project].[ProjectWorkBreakdownStructure]");
            await conn.ExecuteAsync("DELETE FROM [Project].[ProjectMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 20, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var id = await SeedWbsAsync(projectId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(id);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 20, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId, "Foundation Work");
            await SeedWbsAsync(projectId, "Electrical Work");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 20, "Foundation");

            items.Should().HaveCount(1);
            items.ToList()[0].WorkBreakdownStructureName.Should().Be("Foundation Work");
        }

        // --- GET BY PROJECT ---

        [Fact]
        public async Task GetByProjectAsync_Should_Return_WBSForProject()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId, "WBS Task 1");
            await SeedWbsAsync(projectId, "WBS Task 2");

            var result = await CreateQueryRepo().GetByProjectAsync(projectId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByProjectAsync_Should_Return_Empty_For_NonExistent_Project()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByProjectAsync(9999);

            result.Should().BeEmpty();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var id = await SeedWbsAsync(projectId, "Civil Work");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.WorkBreakdownStructureName.Should().Be("Civil Work");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetAutocompleteAsync_Should_Return_Matching_Items()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId, "Foundation Work");
            await SeedWbsAsync(projectId, "Electrical Work");

            var result = await CreateQueryRepo().GetAutocompleteAsync(projectId, "Found");

            result.Should().HaveCount(1);
            result.ToList()[0].WorkBreakdownStructureName.Should().Be("Foundation Work");
        }

        // --- WBS LOOKUP ---

        [Fact]
        public async Task GetWbsLookupAsync_Should_Return_Active_Items()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId, "Foundation Work");

            var result = await CreateQueryRepo().GetWbsLookupAsync(projectId);

            result.Should().HaveCount(1);
        }

        // --- IS NAME UNIQUE ---

        [Fact]
        public async Task IsNameUniqueAsync_Should_Return_False_When_Name_Exists()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            await SeedWbsAsync(projectId, "Foundation Work");

            var isUnique = await CreateQueryRepo().IsNameUniqueAsync(projectId, "Foundation Work");

            isUnique.Should().BeFalse();
        }

        [Fact]
        public async Task IsNameUniqueAsync_Should_Return_True_For_NonExistent()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();

            var isUnique = await CreateQueryRepo().IsNameUniqueAsync(projectId, "Nonexistent WBS");

            isUnique.Should().BeTrue();
        }
    }
}
