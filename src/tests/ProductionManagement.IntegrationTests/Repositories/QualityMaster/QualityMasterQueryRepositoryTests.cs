using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.QualityMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.QualityMaster
{
    [Collection("DatabaseCollection")]
    public sealed class QualityMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QualityMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string name = "Premium", string desc = "Premium quality")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new QualityMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.QualityMaster
            {
                QualityName = name,
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
            await new QualityMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("Premium", "Premium quality");
            await SeedAsync("Standard", "Standard quality");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Standard");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Premium", "Premium quality");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.QualityName.Should().Be("Premium");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new QualityMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("Premium");
            await SeedAsync("Standard");

            var results = await CreateQueryRepo().AutocompleteAsync("Prem", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Premium");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.QualityMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Prem", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- EXISTS / NOT FOUND ---

        [Fact]
        public async Task QualityNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("Premium");

            var exists = await CreateQueryRepo().QualityNameExistsAsync("Premium");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task QualityNameExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Premium");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new QualityMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().QualityNameExistsAsync("Premium");

            exists.Should().BeFalse();
        }

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
