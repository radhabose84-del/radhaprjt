using Dapper;
using Microsoft.Data.SqlClient;
using ProjectManagement.Domain.Common;
using ProjectManagement.Infrastructure.Repositories.MiscMaster;
using ProjectManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace ProjectManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedMiscTypeAsync(string code = "PROJ_STATUS")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var entity = new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Project Status",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "OPEN", string desc = "Open")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var entity = new ProjectManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = desc,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Project].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Project].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Correct_Fields()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId, "CLOSED", "Closed");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items[0].Code.Should().Be("CLOSED");
            items[0].Description.Should().Be("Closed");
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new ProjectManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id, toDelete);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId, "OPEN", "Open Status");
            await SeedMiscMasterAsync(miscTypeId, "CLOSED", "Closed Status");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "OPEN");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("OPEN");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId, "OPEN", "Open");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Code.Should().Be("OPEN");
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId, "OPEN");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("OPEN", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NONEXISTENT", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId, "OPEN");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("OPEN", miscTypeId, id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Record_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }
    }
}
