using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.PriceGroupMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.PriceGroupMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PriceGroupMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PriceGroupMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PriceGroupMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string code = "PGQ1",
            string? name = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                PriceGroupCode = code,
                PriceGroupName = name ?? $"Name {code}",
                Description = "desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.PriceGroupMaster.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_TotalCount()
        {
            await ClearAsync();
            await SeedAsync();

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync(code: "PGQ-DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync(code: "PGQ-A", name: "Alpha");
            await SeedAsync(code: "PGQ-B", name: "Beta");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "Alpha");

            rows.Should().HaveCount(1);
            rows[0].PriceGroupName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllAsync_Should_Respect_Pagination()
        {
            await ClearAsync();
            for (int i = 0; i < 5; i++) await SeedAsync(code: $"PGQ-P{i}");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null);

            rows.Should().HaveCount(2);
            total.Should().Be(5);
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync(code: "PGQ-ID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.PriceGroupCode.Should().Be("PGQ-ID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetByIdAsync(999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync(code: "PGQ-SD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AutocompleteAsync ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Active_Records()
        {
            await ClearAsync();
            await SeedAsync(code: "AC-A", name: "Autocomplete A");
            await SeedAsync(code: "AC-B", name: "Autocomplete B");
            await SeedAsync(code: "OTHER");

            var result = await CreateRepo().AutocompleteAsync("AC-", CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync(code: "ACT");
            await SeedAsync(code: "INA", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("A", CancellationToken.None);

            result.Should().OnlyContain(r => r.PriceGroupCode != "INA");
        }

        // --- AlreadyExistsAsync / NameAlreadyExistsAsync ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Existing_Code()
        {
            await ClearAsync();
            await SeedAsync(code: "EXIST");

            var result = await CreateRepo().AlreadyExistsAsync("EXIST");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted_Code()
        {
            await ClearAsync();
            await SeedAsync(code: "SD-EXIST", deleted: IsDelete.Deleted);

            var result = await CreateRepo().AlreadyExistsAsync("SD-EXIST");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearAsync();
            var id = await SeedAsync(code: "SELF");

            var result = await CreateRepo().AlreadyExistsAsync("SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameAlreadyExistsAsync_Should_Return_True_For_Existing_Name()
        {
            await ClearAsync();
            await SeedAsync(code: "N1", name: "NameDup");

            var result = await CreateRepo().NameAlreadyExistsAsync("NameDup");

            result.Should().BeTrue();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedAsync(code: "NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(999999);

            result.Should().BeTrue();
        }
    }
}
