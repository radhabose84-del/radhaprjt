using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.ProjectMaster;

namespace ProjectManagement.IntegrationTests.Repositories.ProjectMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ProjectMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProjectMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ProjectMasterQueryRepository CreateQueryRepo(
            Mock<IMiscMasterQueryRepository>? miscMock = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);

            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);

            var deptLookup = new Mock<IDepartmentLookup>(MockBehavior.Loose);

            miscMock ??= new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);

            return new ProjectMasterQueryRepository(conn, ipMock.Object, deptLookup.Object, miscMock.Object);
        }

        private async Task<int> SeedProjectAsync(
            string code = "PRJ001",
            string name = "Test Project",
            int unitId = 1,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var project = new ProjectManagement.Domain.Entities.ProjectMaster
            {
                ProjectCode = code,
                ProjectName = name,
                ProjectTypeId = 1,
                UnitId = unitId,
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
                IsActive = isActive ? BaseEntity.Status.Active : BaseEntity.Status.Inactive,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ProjectMaster.Add(project);
            await ctx.SaveChangesAsync();
            return project.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Project].[ProjectWorkBreakdownStructure]");
            await conn.ExecuteAsync("DELETE FROM [Project].[ProjectDocument]");
            await conn.ExecuteAsync("DELETE FROM [Project].[ProjectMaster]");
        }

        // --- GET ALL (GetProjectmasterAsync) ---

        [Fact]
        public async Task GetProjectmasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedProjectAsync();

            var (items, total) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetProjectmasterAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PROJ-TEST", "My Test Project");

            var (items, _) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, null, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].ProjectCode.Should().Be("PROJ-TEST");
            items[0].ProjectName.Should().Be("My Test Project");
        }

        [Fact]
        public async Task GetProjectmasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedProjectAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var project = await ctx.ProjectMaster.FindAsync(id);
            project!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var (items, total) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetProjectmasterAsync_Should_Filter_By_UnitId()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Unit 1 Project", unitId: 1);
            await SeedProjectAsync("PRJ002", "Unit 2 Project", unitId: 2);

            // Mock returns UnitId=1, so only the Unit 1 project should appear
            var (items, total) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].ProjectName.Should().Be("Unit 1 Project");
        }

        [Fact]
        public async Task GetProjectmasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Alpha Project");
            await SeedProjectAsync("PRJ002", "Beta Project");

            var (items, total) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, "Alpha", CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].ProjectName.Should().Be("Alpha Project");
        }

        [Fact]
        public async Task GetProjectmasterAsync_Should_Return_Empty_Documents_When_None_Seeded()
        {
            await ClearTablesAsync();
            await SeedProjectAsync();

            var (items, _) = await CreateQueryRepo()
                .GetProjectmasterAsync(1, 20, null, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].Documents.Should().BeEmpty();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var id = await SeedProjectAsync("PRJ-BYID", "Project By Id");

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.ProjectCode.Should().Be("PRJ-BYID");
            result.ProjectName.Should().Be("Project By Id");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedProjectAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var project = await ctx.ProjectMaster.FindAsync(id);
            project!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Include_Empty_Documents_List()
        {
            await ClearTablesAsync();
            var id = await SeedProjectAsync();

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Documents.Should().NotBeNull();
            result.Documents.Should().BeEmpty();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetProjectMasterAutoCompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Alpha Construction");
            await SeedProjectAsync("PRJ002", "Beta Renovation");

            var result = await CreateQueryRepo()
                .GetProjectMasterAutoCompleteAsync(1, null, "Alpha", 100, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ProjectName.Should().Be("Alpha Construction");
        }

        [Fact]
        public async Task GetProjectMasterAutoCompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Active Project", isActive: true);
            await SeedProjectAsync("PRJ002", "Inactive Project", isActive: false);

            var result = await CreateQueryRepo()
                .GetProjectMasterAutoCompleteAsync(1, null, null, 100, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ProjectName.Should().Be("Active Project");
        }

        [Fact]
        public async Task GetProjectMasterAutoCompleteAsync_Should_Filter_By_UnitId()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Unit1 Project", unitId: 1);
            await SeedProjectAsync("PRJ002", "Unit2 Project", unitId: 2);

            var result = await CreateQueryRepo()
                .GetProjectMasterAutoCompleteAsync(1, null, null, 100, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ProjectName.Should().Be("Unit1 Project");
        }

        [Fact]
        public async Task GetProjectMasterAutoCompleteAsync_Should_Return_Empty_When_No_Match()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Some Project");

            var result = await CreateQueryRepo()
                .GetProjectMasterAutoCompleteAsync(1, null, "NonExistentXYZ", 100, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProjectMasterAutoCompleteAsync_Should_Return_Empty_For_Status_Keyword_With_No_Matching_Projects()
        {
            await ClearTablesAsync();
            await SeedProjectAsync("PRJ001", "Active Project");

            // "Approved" is a status keyword — misc repo returns a MiscMaster with ID 9999
            // No project has StatusId=9999, so result is empty
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ProjectManagement.Domain.Entities.MiscMaster
                {
                    Id = 9999,
                    Code = "APPROVED",
                    Description = "Approved"
                });

            var result = await CreateQueryRepo(miscMock)
                .GetProjectMasterAutoCompleteAsync(1, null, "Approved", 100, CancellationToken.None);

            result.Should().BeEmpty();
        }

        // --- GET PROJECT NAME ---

        [Fact]
        public async Task GetProjectNameAsync_Should_Return_Name_When_Found()
        {
            await ClearTablesAsync();
            var id = await SeedProjectAsync("PRJ001", "Named Project");

            var name = await CreateQueryRepo().GetProjectNameAsync(id, CancellationToken.None);

            name.Should().Be("Named Project");
        }

        [Fact]
        public async Task GetProjectNameAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var name = await CreateQueryRepo().GetProjectNameAsync(9999, CancellationToken.None);

            name.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectNameAsync_Should_Return_Null_For_ZeroId()
        {
            await ClearTablesAsync();

            var name = await CreateQueryRepo().GetProjectNameAsync(0, CancellationToken.None);

            name.Should().BeNull();
        }
    }
}
