using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure;

namespace ProjectManagement.IntegrationTests.Repositories.ProjectWorkBreakdownStructure
{
    /// <summary>
    /// Extended query repository tests covering NotFoundAsync, SoftDeleteValidationAsync,
    /// GetParentLevelAsync, and pagination — methods not covered in ProjectWBSQueryRepositoryTests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProjectWorkBreakdownStructureQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectWorkBreakdownStructureQueryRepositoryTests(DbFixture fixture)
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

        private async Task<int> SeedProjectAsync(string code = "PRJ001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var project = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = code,
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

        private async Task<int> SeedWbsAsync(
            int projectId,
            string name = "Foundation Work",
            int? parentId = null,
            int level = 1,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
            {
                ProjectId = projectId,
                ParentWorkBreakdownScheduleIIIMasterId = parentId,
                WorkBreakdownStructureName = name,
                ResponsibleDepartmentId = 1,
                ResponsiblePerson = "John Doe",
                CurrencyId = 1,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                Level = level,
                IsActive = isActive ? BaseEntity.Status.Active : BaseEntity.Status.Inactive,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await CreateCommandRepo(ctx).AddAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Id_DoesNot_Exist()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Id_Exists()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var wbsId = await SeedWbsAsync(projectId);

            var result = await CreateQueryRepo().NotFoundAsync(wbsId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var wbsId = await SeedWbsAsync(projectId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(wbsId);

            var result = await CreateQueryRepo().NotFoundAsync(wbsId);

            result.Should().BeTrue();
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var parentId = await SeedWbsAsync(projectId, "Parent WBS");

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(parentId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Active_Children_Exist()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var parentId = await SeedWbsAsync(projectId, "Parent WBS");
            await SeedWbsAsync(projectId, "Child WBS", parentId: parentId, level: 2);

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(parentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_All_Children_Deleted()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var parentId = await SeedWbsAsync(projectId, "Parent WBS");
            var childId = await SeedWbsAsync(projectId, "Child WBS", parentId: parentId, level: 2);

            // Soft-delete the child
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).DeleteAsync(childId);

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(parentId);

            result.Should().BeFalse();
        }

        // --- GetParentLevelAsync ---

        [Fact]
        public async Task GetParentLevelAsync_Should_Return_Level_Of_Parent()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var parentId = await SeedWbsAsync(projectId, "Parent", level: 3);

            var level = await CreateQueryRepo().GetParentLevelAsync(parentId);

            level.Should().Be(3);
        }

        [Fact]
        public async Task GetParentLevelAsync_Should_Return_Default_1_When_NotFound()
        {
            await ClearTablesAsync();

            var level = await CreateQueryRepo().GetParentLevelAsync(9999);

            level.Should().Be(1);
        }

        // --- GetAllAsync pagination ---

        [Fact]
        public async Task GetAllAsync_Should_Paginate_Correctly()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            for (int i = 1; i <= 5; i++)
                await SeedWbsAsync(projectId, $"WBS Task {i}");

            var (page1, total1) = await CreateQueryRepo().GetAllAsync(1, 2, null);
            var (page2, total2) = await CreateQueryRepo().GetAllAsync(2, 2, null);

            total1.Should().Be(5);
            total2.Should().Be(5);
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_No_Records()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 20, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GetAutocompleteAsync ---

        [Fact]
        public async Task GetAutocompleteAsync_Should_Return_All_When_No_ProjectFilter()
        {
            await ClearTablesAsync();
            var proj1 = await SeedProjectAsync("P1");
            var proj2 = await SeedProjectAsync("P2");
            await SeedWbsAsync(proj1, "WBS A");
            await SeedWbsAsync(proj2, "WBS B");

            var result = await CreateQueryRepo().GetAutocompleteAsync(null, null);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAutocompleteAsync_Should_Filter_By_ProjectId()
        {
            await ClearTablesAsync();
            var proj1 = await SeedProjectAsync("P1");
            var proj2 = await SeedProjectAsync("P2");
            await SeedWbsAsync(proj1, "WBS A");
            await SeedWbsAsync(proj2, "WBS B");

            var result = await CreateQueryRepo().GetAutocompleteAsync(proj1, null);

            result.Should().HaveCount(1);
            result[0].WorkBreakdownStructureName.Should().Be("WBS A");
        }

        // --- IsNameUniqueAsync with excludeId ---

        [Fact]
        public async Task IsNameUniqueAsync_Should_Return_True_When_Excluding_Self()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var wbsId = await SeedWbsAsync(projectId, "Unique Name");

            var isUnique = await CreateQueryRepo().IsNameUniqueAsync(projectId, "Unique Name", excludeId: wbsId);

            isUnique.Should().BeTrue();
        }

        [Fact]
        public async Task IsNameUniqueAsync_Should_Return_False_When_Another_Record_Has_Same_Name()
        {
            await ClearTablesAsync();
            var projectId = await SeedProjectAsync();
            var wbs1 = await SeedWbsAsync(projectId, "Shared Name");
            var wbs2 = await SeedWbsAsync(projectId, "Other Name");

            var isUnique = await CreateQueryRepo().IsNameUniqueAsync(projectId, "Shared Name", excludeId: wbs2);

            isUnique.Should().BeFalse();
        }
    }
}
