using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MixCodeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.MixCodeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MixCodeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MixCodeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MixCodeMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private static MixCodeMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedAsync(string code = "MIX001", string desc = "Test Mix", bool active = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new PurchaseManagement.Domain.Entities.MixCodeMaster
            {
                MixCode = code,
                MixCodeDesc = desc,
                IsActive = active ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("MIX001", "Alpha Mix");
            await SeedAsync("MIX002", "Beta Mix");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].MixCodeDesc.Should().Be("Alpha Mix");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync("MIX010", "Lookup Mix");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.MixCode.Should().Be("MIX010");
            dto.MixCodeDesc.Should().Be("Lookup Mix");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("MIX001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MIX001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync("MIX001");
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MIX001");

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("MIX001", "Active Mix", active: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Match()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("MIX001", "Active Mix");

            var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        // --- NOT FOUND / RULE #25 GUARD ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Absent()
        {
            await _fixture.ClearAllTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_ArrivalDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAsync();

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            linked.Should().BeFalse();
        }
    }
}
