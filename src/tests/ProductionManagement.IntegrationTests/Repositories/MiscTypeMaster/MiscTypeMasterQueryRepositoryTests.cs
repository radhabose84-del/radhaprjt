using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.MiscTypeMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscTypeMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string code = "MT001", string desc = "Misc type one")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Production.ProductionPackDetail");
            await conn.ExecuteAsync("DELETE FROM Production.ProductionPackHeader");
            await conn.ExecuteAsync("DELETE FROM Production.LotMaster");
            await conn.ExecuteAsync("DELETE FROM Production.LotMaster");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscTypeMaster]");
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
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("MT001", "Color Type");
            await SeedAsync("MT002", "Size Type");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Size");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("MT001", "Color Type");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.MiscTypeCode.Should().Be("MT001");
            dto.Description.Should().Be("Color Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("MT001", "Color Type");
            await SeedAsync("MT002", "Size Type");

            var results = await CreateQueryRepo().AutocompleteAsync("Color", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("MT001", "Color Type");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Color", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("MT001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MT001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("MT001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MT001");

            exists.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearTableAsync();
            var id = await SeedAsync("MT001");

            var hasChildren = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            hasChildren.Should().BeFalse();
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
