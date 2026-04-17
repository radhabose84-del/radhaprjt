using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private async Task<Domain.Entities.Notification.MiscTypeMaster> SeedEntityAsync(
            string miscTypeCode = "TESTTYPE",
            string description = "Test MiscType")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Notification.MiscTypeMaster
            {
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.MiscTypeMaster { IsDeleted = IsDelete.Deleted };
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(entity.Id, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("ALPHA", "Alpha Type");
            await SeedEntityAsync("BETA", "Beta Type");

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("ALPHA");
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Pagination()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("TYPE01", "Type 1");
            await SeedEntityAsync("TYPE02", "Type 2");
            await SeedEntityAsync("TYPE03", "Type 3");

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync("GETID01", "Get By Id Test");

            var result = await CreateQueryRepo().GetByIdAsync(entity.Id);

            result.Should().NotBeNull();
            result.MiscTypeCode.Should().Be("GETID01");
            result.Description.Should().Be("Get By Id Test");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.MiscTypeMaster { IsDeleted = IsDelete.Deleted };
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(entity.Id, deleteEntity);

            var result = await CreateQueryRepo().GetByIdAsync(entity.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("EXISTS01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("EXISTS01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOTEXIST");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync("SELF01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("SELF01", entity.Id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync("DEL01");

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.MiscTypeMaster { IsDeleted = IsDelete.Deleted };
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(entity.Id, deleteEntity);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DEL01");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var entity = await SeedEntityAsync();

            var result = await CreateQueryRepo().NotFoundAsync(entity.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }

        // --- AUTOCOMPLETE (GetMiscTypeMaster) ---

        [Fact]
        public async Task GetMiscTypeMaster_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("AUTO01", "Auto Complete");
            await SeedEntityAsync("NOMATCH", "No Match");

            var result = await CreateQueryRepo().GetMiscTypeMaster("AUTO");

            result.Should().HaveCount(1);
            result[0].MiscTypeCode.Should().Be("AUTO01");
        }

        [Fact]
        public async Task GetMiscTypeMaster_Should_Return_All_When_Empty_Pattern()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("TYPE01", "Type One");
            await SeedEntityAsync("TYPE02", "Type Two");

            var result = await CreateQueryRepo().GetMiscTypeMaster("");

            result.Should().HaveCount(2);
        }
    }
}
