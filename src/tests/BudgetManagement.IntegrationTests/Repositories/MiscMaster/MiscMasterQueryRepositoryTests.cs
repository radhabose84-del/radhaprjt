using BudgetManagement.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BudgetManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetManagement.Infrastructure.Repositories.MiscMaster.MiscMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private MiscMasterCommandRepository CreateCommandRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private MiscTypeMasterCommandRepository CreateMiscTypeRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeAsync(string code = "MT001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await CreateMiscTypeRepo(ctx).CreateAsync(
                new BudgetManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Test Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedEntityAsync(int miscTypeId, string code = "MSC001", string description = "Test")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await CreateCommandRepo(ctx).CreateAsync(
                new BudgetManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MSC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new BudgetManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId, "ALPHA01", "Alpha Misc");
            await SeedEntityAsync(miscTypeId, "BETA001", "Beta Misc");

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("ALPHA01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId, "CODE001");
            await SeedEntityAsync(miscTypeId, "CODE002");
            await SeedEntityAsync(miscTypeId, "CODE003");

            var (page1Items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 2, null);

            page1Items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MSC001", "My Misc");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Code.Should().Be("MSC001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            // Returns new entity (Id = 0) per implementation
            result.Id.Should().Be(0);
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId, "MSC001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MSC001", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOTHERE", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MSC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new BudgetManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, deleteEntity);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MSC001", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_ExcludingSelf()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MSC001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MSC001", miscTypeId, id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId);

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }
    }
}
