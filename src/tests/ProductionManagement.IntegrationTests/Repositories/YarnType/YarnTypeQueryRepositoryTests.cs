using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.YarnType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.YarnType
{
    [Collection("DatabaseCollection")]
    public sealed class YarnTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public YarnTypeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private YarnTypeQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string code = "YT001",
            string name = "Cotton",
            string desc = "Cotton yarn")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new YarnTypeCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.YarnType
            {
                YarnTypeCode = code,
                YarnTypeName = name,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[YarnType]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton", "Cotton yarn");
            await SeedAsync("YT002", "Polyester", "Polyester yarn");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Polyester");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton", "Cotton yarn");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.YarnTypeCode.Should().Be("YT001");
            dto.YarnTypeName.Should().Be("Cotton");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton");
            await SeedAsync("YT002", "Polyester");

            var results = await CreateQueryRepo().AutocompleteAsync("Cotton", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.YarnType.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Cotton", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("YT001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("YT001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("YT001");

            exists.Should().BeFalse();
        }

        // --- YARN TYPE NAME EXISTS ---

        [Fact]
        public async Task YarnTypeNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton");

            var exists = await CreateQueryRepo().YarnTypeNameExistsAsync("Cotton");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task YarnTypeNameExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().YarnTypeNameExistsAsync("NonExistent");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }
    }
}
