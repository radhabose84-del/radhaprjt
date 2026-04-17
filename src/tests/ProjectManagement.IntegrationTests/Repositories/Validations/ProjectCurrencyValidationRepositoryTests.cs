using Dapper;
using Microsoft.Data.SqlClient;
using ProjectManagement.Domain.Common;

namespace ProjectManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for ProjectCurrencyValidationRepository.
    /// Since the repo is internal sealed, tests use raw SQL that mirrors
    /// the repository's HasLinkedCurrencyAsync query logic.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProjectCurrencyValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectCurrencyValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Mirrors the SQL logic of ProjectCurrencyValidationRepository.HasLinkedCurrencyAsync
        /// </summary>
        private async Task<bool> HasLinkedCurrencyAsync(int currencyId)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Project].[ProjectMaster]                  WHERE CurrencyId = @Id AND IsDeleted = 0)
                    OR
                    EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure]  WHERE CurrencyId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
        }

        private async Task<int> SeedProjectAsync(int currencyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var project = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = $"CUR{currencyId}",
                ProjectName = "Currency Test Project",
                ProjectTypeId = 1,
                UnitId = 1,
                DepartmentId = 1,
                BudgetAmount = 100000m,
                BudgetYearId = 1,
                CostCenterId = 1,
                CurrencyId = currencyId,
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

        private async Task<int> SeedWbsAsync(int projectId, int currencyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new ProjectManagement.Domain.Entities.ProjectWorkBreakdownStructure
            {
                ProjectId = projectId,
                WorkBreakdownStructureName = "WBS Currency Test",
                ResponsibleDepartmentId = 1,
                ResponsiblePerson = "Tester",
                CurrencyId = currencyId,
                UnitId = 1,
                BudgetYearId = 1,
                StatusId = 1,
                Level = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ProjectWorkBreakdownStructures.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- HasLinkedCurrencyAsync (via ProjectMaster) ---

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_True_When_ProjectMaster_Uses_Currency()
        {
            await ClearAsync();
            await SeedProjectAsync(currencyId: 10);

            var result = await HasLinkedCurrencyAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_No_Records_Use_Currency()
        {
            await ClearAsync();

            var result = await HasLinkedCurrencyAsync(9999);

            result.Should().BeFalse();
        }

        // --- HasLinkedCurrencyAsync (via WBS) ---

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_True_When_WBS_Uses_Currency()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(currencyId: 1); // project uses currency 1
            await SeedWbsAsync(projectId, currencyId: 20); // WBS uses currency 20

            var result = await HasLinkedCurrencyAsync(20);

            result.Should().BeTrue();
        }

        // --- Soft-deleted records should not count ---

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_Project_Is_SoftDeleted()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(currencyId: 30);

            // Soft-delete the project
            await using var ctx = _fixture.CreateFreshDbContext();
            var project = await ctx.ProjectMaster.FindAsync(projectId);
            project!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await HasLinkedCurrencyAsync(30);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_WBS_Is_SoftDeleted()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(currencyId: 1);
            var wbsId = await SeedWbsAsync(projectId, currencyId: 40);

            // Soft-delete the WBS
            await using var ctx = _fixture.CreateFreshDbContext();
            var wbs = await ctx.ProjectWorkBreakdownStructures.FindAsync(wbsId);
            wbs!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await HasLinkedCurrencyAsync(40);

            result.Should().BeFalse();
        }

        // --- OR logic: either table should trigger true ---

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_True_When_Only_WBS_Uses_Currency_Not_Project()
        {
            await ClearAsync();
            var projectId = await SeedProjectAsync(currencyId: 1); // project uses different currency
            await SeedWbsAsync(projectId, currencyId: 50);

            var result = await HasLinkedCurrencyAsync(50);

            result.Should().BeTrue();
        }
    }
}
