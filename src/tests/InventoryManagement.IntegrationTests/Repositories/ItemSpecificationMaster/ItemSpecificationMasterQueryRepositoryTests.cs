using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.ItemSpecificationMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemSpecificationMasterTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSpecificationMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSpecificationMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSpecificationMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string code,
            string name,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var maxOrder = await ctx.ItemSpecificationMaster.AnyAsync()
                ? await ctx.ItemSpecificationMaster.MaxAsync(m => m.Order)
                : 0;
            var m = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster
            {
                SpecificationCode = code,
                SpecificationName = name,
                Order = maxOrder + 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ItemSpecificationMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_TotalCount()
        {
            await ClearAsync();
            await SeedAsync("QA1", "Name QA1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DEL1", "DelName", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNIQ1", "Unique Name");
            await SeedAsync("OTHQ1", "Other");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ");

            rows.Should().HaveCount(1);
            rows[0].SpecificationCode.Should().Be("UNIQ1");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("GBID1", "GbName");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.SpecificationCode.Should().Be("GBID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("GBSD", "Del", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AutocompleteAsync ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("ACQ1", "Active Q1");
            await SeedAsync("INQ1", "Inactive Q1", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("Q1", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].SpecificationCode.Should().Be("ACQ1");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_All_When_Term_Empty()
        {
            await ClearAsync();
            await SeedAsync("A1", "A");
            await SeedAsync("B1", "B");

            var result = await CreateRepo().AutocompleteAsync(string.Empty, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        // --- AlreadyExistsAsync / NameAlreadyExistsAsync / OrderAlreadyExistsAsync ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("DUP1", "DupName");

            var result = await CreateRepo().AlreadyExistsAsync("DUP1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearAsync();
            var id = await SeedAsync("SELF1", "SelfName");

            var result = await CreateRepo().AlreadyExistsAsync("SELF1", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameAlreadyExistsAsync_Should_Detect_Duplicate_Name()
        {
            await ClearAsync();
            await SeedAsync("N1", "DupName");

            var result = await CreateRepo().NameAlreadyExistsAsync("DupName");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task OrderAlreadyExistsAsync_Should_Detect_Duplicate_Order()
        {
            await ClearAsync();
            var id = await SeedAsync("O1", "OrderA");
            int orderA;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                orderA = (await ctx.ItemSpecificationMaster.FirstAsync(m => m.Id == id)).Order;
            }

            var result = await CreateRepo().OrderAlreadyExistsAsync(orderA);

            result.Should().BeTrue();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedAsync("NF1", "Name");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedAsync("SDV1", "Name");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Linked_By_SpecValue()
        {
            await ClearAsync();
            var id = await SeedAsync("SDV2", "Name");
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.ItemSpecificationValue.AddAsync(new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = id,
                SpecificationValue = "v",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        // --- IsItemSpecificationMasterLinkedAsync ---

        [Fact]
        public async Task IsItemSpecificationMasterLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedAsync("LK1", "Name");

            var result = await CreateRepo().IsItemSpecificationMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
