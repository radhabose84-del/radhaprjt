using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.CertificationMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CertificationMaster
{
    [Collection("DatabaseCollection")]
    public sealed class CertificationMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CertificationMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CertificationMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string name = "ISO 9001", string desc = "Quality")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CertificationMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.CertificationMaster
            {
                CertificationName = name,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[CertificationMaster]");
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
            await new CertificationMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("ISO 9001", "Quality");
            await SeedAsync("ISO 14001", "Environmental");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "14001");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("ISO 9001", "Quality Mgmt");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.CertificationName.Should().Be("ISO 9001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CertificationMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("ISO 9001");
            await SeedAsync("OHSAS 18001");

            var results = await CreateQueryRepo().AutocompleteAsync("ISO", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("ISO 9001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.CertificationMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("ISO", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- EXISTS / NOT FOUND ---

        [Fact]
        public async Task CertificationNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("ISO 9001");

            var exists = await CreateQueryRepo().CertificationNameExistsAsync("ISO 9001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CertificationNameExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("ISO 9001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CertificationMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().CertificationNameExistsAsync("ISO 9001");

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
