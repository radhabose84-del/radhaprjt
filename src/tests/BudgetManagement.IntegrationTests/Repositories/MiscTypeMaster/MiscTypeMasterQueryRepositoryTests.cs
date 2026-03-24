using BudgetManagement.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BudgetManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private MiscTypeMasterCommandRepository CreateCommandRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedEntityAsync(string code = "MTT001", string description = "Test")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await CreateCommandRepo(ctx).CreateAsync(
                new BudgetManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Budget.MiscTypeMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ALPHA01", "Alpha Type");
            await SeedEntityAsync("BETA001", "Beta Type");

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("ALPHA01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("CODE001", "First");
            await SeedEntityAsync("CODE002", "Second");
            await SeedEntityAsync("CODE003", "Third");

            var (page1Items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 2, null);

            page1Items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MTT001", "My Type");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.MiscTypeCode.Should().Be("MTT001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            // Returns new entity (not null) per implementation
            result.Id.Should().Be(0);
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("MTT001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MTT001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOTHERE");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MTT001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, deleteEntity);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MTT001");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_ExcludingSelf()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MTT001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MTT001", id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_NotExists()
        {
            await ClearTableAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }
    }
}
