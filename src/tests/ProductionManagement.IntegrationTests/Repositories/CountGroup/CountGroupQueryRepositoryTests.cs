using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.CountGroup;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CountGroup
{
    [Collection("DatabaseCollection")]
    public sealed class CountGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CountGroupQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountGroupQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string code = "CG001",
            string name = "Fine Count",
            string desc = "Fine count group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CountGroupCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.CountGroup
            {
                CountGroupCode = code,
                CountGroupName = name,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

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
            await new CountGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("CG001", "Fine Count", "Fine");
            await SeedAsync("CG002", "Coarse Count", "Coarse");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Coarse");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("CG001", "Fine Count", "Fine group");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.CountGroupCode.Should().Be("CG001");
            dto.CountGroupName.Should().Be("Fine Count");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CountGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("CG001", "Fine Count");
            await SeedAsync("CG002", "Coarse Count");

            var results = await CreateQueryRepo().AutocompleteAsync("Fine", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("CG001", "Fine Count");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.CountGroup.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Fine", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("CG001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("CG001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("CG001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CountGroupCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("CG001");

            exists.Should().BeFalse();
        }

        // --- COUNT GROUP NAME EXISTS ---

        [Fact]
        public async Task CountGroupNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("CG001", "Fine Count");

            var exists = await CreateQueryRepo().CountGroupNameExistsAsync("Fine Count");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CountGroupNameExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().CountGroupNameExistsAsync("NonExistent");

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
