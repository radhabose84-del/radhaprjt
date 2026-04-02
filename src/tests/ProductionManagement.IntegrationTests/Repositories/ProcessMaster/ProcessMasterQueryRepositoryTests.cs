using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.ProcessMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.ProcessMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ProcessMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProcessMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProcessMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string name = "Spinning",
            bool combingRequired = false,
            string desc = "Spinning process")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ProcessMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.ProcessMaster
            {
                ProcessName = name,
                CombingRequired = combingRequired,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[ProcessMaster]");
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
            await new ProcessMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("Spinning", false, "Spinning process");
            await SeedAsync("Weaving", true, "Weaving process");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Weaving");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Spinning", true, "Spinning process");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ProcessName.Should().Be("Spinning");
            dto.CombingRequired.Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ProcessMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("Spinning");
            await SeedAsync("Weaving");

            var results = await CreateQueryRepo().AutocompleteAsync("Spin", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Spinning");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.ProcessMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Spin", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- EXISTS / NOT FOUND ---

        [Fact]
        public async Task ProcessNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("Spinning");

            var exists = await CreateQueryRepo().ProcessNameExistsAsync("Spinning");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessNameExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("Spinning");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ProcessMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().ProcessNameExistsAsync("Spinning");

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
