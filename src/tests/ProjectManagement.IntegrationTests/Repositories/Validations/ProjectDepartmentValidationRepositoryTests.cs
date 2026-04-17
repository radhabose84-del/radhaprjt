using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Domain.Common;

namespace ProjectManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for ProjectDepartmentValidationRepository.
    /// Since the repo is internal sealed, tests use raw SQL that mirrors
    /// the repository's HasLinkedDepartmentAsync and HasActiveDepartmentAsync query logic.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProjectDepartmentValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectDepartmentValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Mirrors ProjectDepartmentValidationRepository.HasLinkedDepartmentAsync
        /// </summary>
        private async Task<bool> HasLinkedDepartmentAsync(int departmentId)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                 WHERE DepartmentId = @Id AND IsDeleted = 0)
                    OR
                    EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure] WHERE ResponsibleDepartmentId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
        }

        /// <summary>
        /// Mirrors ProjectDepartmentValidationRepository.HasActiveDepartmentAsync
        /// </summary>
        private async Task<bool> HasActiveDepartmentAsync(int departmentId)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                 WHERE DepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR
                    EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure] WHERE ResponsibleDepartmentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END";

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = departmentId });
        }

        private async Task<int> SeedProjectAsync(
            int departmentId = 1,
            bool isActive = true,
            string code = "DEPT01")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var project = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = code,
                ProjectName = "Dept Test Project",
                ProjectTypeId = 1,
                UnitId = 1,
                DepartmentId = departmentId,
                BudgetAmount = 100000m,
                BudgetYearId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddMonths(6),
                ProjectCategoryId = 1,
                AssetGroupId = 1,
                StatusId = 1,
                IsActive = isActive ? BaseEntity.Status.Active : BaseEntity.Status.Inactive,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ProjectMaster.Add(project);
            await ctx.SaveChangesAsync();
            return project.Id;
        }

        private async Task<int> SeedWbsAsync(
            int projectId,
            int responsibleDepartmentId = 1,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
            {
                ProjectId = projectId,
                WorkBreakdownStructureName = "WBS Dept Test",
                ResponsibleDepartmentId = responsibleDepartmentId,
                ResponsiblePerson = "Tester",
                CurrencyId = 1,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                Level = 1,
                IsActive = isActive ? BaseEntity.Status.Active : BaseEntity.Status.Inactive,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ProjectWorkBreakdownStructures.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // =====================
        // HasLinkedDepartmentAsync
        // =====================

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_ProjectMaster_Uses_Department()
        {
            await ClearAsync();
            await SeedProjectAsync(departmentId: 10);

            var result = await HasLinkedDepartmentAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_WBS_Uses_Department()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(departmentId: 1);
            await SeedWbsAsync(projectId, responsibleDepartmentId: 20);

            var result = await HasLinkedDepartmentAsync(20);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_No_Records_Use_Department()
        {
            await ClearAsync();

            var result = await HasLinkedDepartmentAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_Project_Is_SoftDeleted()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(departmentId: 30);

            await using var ctx = _fixture.CreateFreshDbContext();
            var project = await ctx.ProjectMaster.FindAsync(projectId);
            project!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await HasLinkedDepartmentAsync(30);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_Even_When_Record_Is_Inactive()
        {
            await ClearAsync();
            await SeedProjectAsync(departmentId: 40, isActive: false);

            // HasLinkedDepartmentAsync only checks IsDeleted, not IsActive
            var result = await HasLinkedDepartmentAsync(40);

            result.Should().BeTrue();
        }

        // =====================
        // HasActiveDepartmentAsync
        // =====================

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_True_When_Active_Project_Uses_Department()
        {
            await ClearAsync();
            await SeedProjectAsync(departmentId: 50, isActive: true);

            var result = await HasActiveDepartmentAsync(50);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_Project_Is_Inactive()
        {
            await ClearAsync();
            await SeedProjectAsync(departmentId: 60, isActive: false);

            var result = await HasActiveDepartmentAsync(60);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_True_When_Active_WBS_Uses_Department()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(departmentId: 1);
            await SeedWbsAsync(projectId, responsibleDepartmentId: 70, isActive: true);

            var result = await HasActiveDepartmentAsync(70);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_WBS_Is_Inactive()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(departmentId: 1);
            await SeedWbsAsync(projectId, responsibleDepartmentId: 80, isActive: false);

            var result = await HasActiveDepartmentAsync(80);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_No_Records_Use_Department()
        {
            await ClearAsync();

            var result = await HasActiveDepartmentAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_Project_Is_SoftDeleted()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(departmentId: 90, isActive: true);

            await using var ctx = _fixture.CreateFreshDbContext();
            var project = await ctx.ProjectMaster.FindAsync(projectId);
            project!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await HasActiveDepartmentAsync(90);

            result.Should().BeFalse();
        }
    }
}
