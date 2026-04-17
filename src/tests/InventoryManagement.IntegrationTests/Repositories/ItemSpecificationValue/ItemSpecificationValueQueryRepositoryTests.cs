using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Repositories.ItemSpecificationValue;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemSpecificationValue
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSpecificationValueQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSpecificationValueQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSpecificationValueQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureSpecMasterAsync(string name = "Color")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemSpecificationMaster.FirstOrDefaultAsync(m => m.SpecificationName == name);
            if (existing != null) return existing.Id;
            var maxOrder = await ctx.ItemSpecificationMaster.AnyAsync()
                ? await ctx.ItemSpecificationMaster.MaxAsync(m => m.Order)
                : 0;
            var m = new ItemSpecificationMaster
            {
                SpecificationCode = name.ToUpper()[..Math.Min(name.Length, 5)],
                SpecificationName = name,
                Order = maxOrder + 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedValueAsync(
            int? specMasterId = null,
            string value = "Red",
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            var masterId = specMasterId ?? await EnsureSpecMasterAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = masterId,
                SpecificationValue = value,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ItemSpecificationValue.AddAsync(v);
            await ctx.SaveChangesAsync();
            return v.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_TotalCount()
        {
            await ClearAsync();
            await SeedValueAsync();

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedValueAsync(deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var masterId = await EnsureSpecMasterAsync("Color");
            await SeedValueAsync(masterId, "Red");
            await SeedValueAsync(masterId, "Blue");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "Red");

            rows.Should().HaveCount(1);
            rows[0].SpecificationValue.Should().Be("Red");
        }

        [Fact]
        public async Task GetAllAsync_Should_Respect_Pagination()
        {
            await ClearAsync();
            var masterId = await EnsureSpecMasterAsync("Color");
            for (int i = 0; i < 5; i++) await SeedValueAsync(masterId, $"V{i}");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null);

            rows.Should().HaveCount(2);
            total.Should().Be(5);
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedValueAsync(value: "Green");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.SpecificationValue.Should().Be("Green");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedValueAsync(deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetBySpecificationMasterIdAsync ---

        [Fact]
        public async Task GetBySpecificationMasterIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var m1 = await EnsureSpecMasterAsync("Color");
            var m2 = await EnsureSpecMasterAsync("Size");
            await SeedValueAsync(m1, "Red");
            await SeedValueAsync(m1, "Blue");
            await SeedValueAsync(m2, "Large");

            var result = await CreateRepo().GetBySpecificationMasterIdAsync(m1, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetBySpecificationMasterIdAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            var m = await EnsureSpecMasterAsync("Color");
            await SeedValueAsync(m, "Active");
            await SeedValueAsync(m, "Inactive", active: Status.Inactive);

            var result = await CreateRepo().GetBySpecificationMasterIdAsync(m, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].SpecificationValue.Should().Be("Active");
        }

        // --- AutocompleteAsync ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Active()
        {
            await ClearAsync();
            await SeedValueAsync(value: "AutoRed");
            await SeedValueAsync(value: "AutoBlue");
            await SeedValueAsync(value: "XY");

            var result = await CreateRepo().AutocompleteAsync("Auto", CancellationToken.None);

            result.Should().HaveCount(2);
        }

        // --- AlreadyExistsAsync ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            var m = await EnsureSpecMasterAsync();
            await SeedValueAsync(m, "DupVal");

            var result = await CreateRepo().AlreadyExistsAsync(m, "DupVal");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearAsync();
            var m = await EnsureSpecMasterAsync();
            var id = await SeedValueAsync(m, "SelfVal");

            var result = await CreateRepo().AlreadyExistsAsync(m, "SelfVal", id);

            result.Should().BeFalse();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedValueAsync();

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(999999);

            result.Should().BeTrue();
        }

        // --- SpecificationMasterExistsAsync ---

        [Fact]
        public async Task SpecificationMasterExistsAsync_Should_Return_True_For_Active_Master()
        {
            var id = await EnsureSpecMasterAsync("ExistsMaster");

            var result = await CreateRepo().SpecificationMasterExistsAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SpecificationMasterExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().SpecificationMasterExistsAsync(999999);

            result.Should().BeFalse();
        }

        // --- SoftDeleteValidationAsync / IsItemSpecificationValueLinkedAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedValueAsync();

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsItemSpecificationValueLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedValueAsync();

            var result = await CreateRepo().IsItemSpecificationValueLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
