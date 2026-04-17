using Microsoft.Data.SqlClient;
using Dapper;
using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string miscTypeCode = "TYPE001", string description = "Test Type")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ALPHA01", "Alpha Type");
            await SeedEntityAsync("BETA001", "Beta Type");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Type");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPE001", "Test Type");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.MiscTypeCode.Should().Be("TYPE001");
            dto.Description.Should().Be("Test Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPE001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("TYPE001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPE001");
            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("TYPE001");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPE001", "Active Type");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
