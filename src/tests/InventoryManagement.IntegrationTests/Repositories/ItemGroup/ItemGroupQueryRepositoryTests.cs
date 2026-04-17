using Contracts.Interfaces;
using Contracts.Interfaces.Validations.WarehouseManagement;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemGroupTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupQueryRepository CreateRepo(
            Mock<IWarehouseItemGroupValidation>? warehouse = null,
            Mock<IDataAccessFilter>? dataAccess = null)
        {
            if (warehouse == null)
            {
                warehouse = new Mock<IWarehouseItemGroupValidation>(MockBehavior.Loose);
                warehouse.Setup(w => w.HasLinkedItemGroupAsync(It.IsAny<int>())).ReturnsAsync(false);
                warehouse.Setup(w => w.HasActiveItemGroupAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (dataAccess == null)
            {
                dataAccess = new Mock<IDataAccessFilter>(MockBehavior.Loose);
                dataAccess.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DataAccessContext.Unrestricted);
            }

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemGroupQueryRepository(conn, warehouse.Object, dataAccess.Object);
        }

        private async Task<int> SeedAsync(
            string code,
            string? name = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = code,
                ItemGroupName = name ?? code,
                UnitId = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("GID1", "Name1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.ItemGroupCode.Should().Be("GID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("GSD1", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetAllItemGroupAsync ---

        [Fact]
        public async Task GetAllItemGroupAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("GA1");

            var (rows, total) = await CreateRepo().GetAllItemGroupAsync(1, 10, null!);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllItemGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("GDEL", deleted: IsDelete.Deleted);

            var (rows, _) = await CreateRepo().GetAllItemGroupAsync(1, 10, "GDEL");

            rows.Should().BeEmpty();
        }

        // --- NotFoundAsync (returns TRUE when exists!) ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Existing()
        {
            await ClearAsync();
            var id = await SeedAsync("NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeFalse();
        }

        // --- GetItemGroupAutoCompleteAsync ---

        [Fact]
        public async Task GetItemGroupAutoCompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("AC1", "Active Group");
            await SeedAsync("IN1", "Inactive Group", active: Status.Inactive);

            var result = await CreateRepo().GetItemGroupAutoCompleteAsync("Group");

            result.Should().ContainSingle();
            result[0].ItemGroupName.Should().Be("Active Group");
        }

        // --- SoftDeleteValidation ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedAsync("SDV1");

            var result = await CreateRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_WarehouseValidation_Links()
        {
            await ClearAsync();
            var id = await SeedAsync("SDV2");
            var wh = new Mock<IWarehouseItemGroupValidation>(MockBehavior.Loose);
            wh.Setup(w => w.HasLinkedItemGroupAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(warehouse: wh).SoftDeleteValidation(id);

            result.Should().BeTrue();
        }

        // --- IsItemGroupLinkedAsync ---

        [Fact]
        public async Task IsItemGroupLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedAsync("LK1");

            var result = await CreateRepo().IsItemGroupLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsItemGroupLinkedAsync_Should_Return_True_When_WarehouseValidation_Active()
        {
            await ClearAsync();
            var id = await SeedAsync("LK2");
            var wh = new Mock<IWarehouseItemGroupValidation>(MockBehavior.Loose);
            wh.Setup(w => w.HasActiveItemGroupAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(warehouse: wh).IsItemGroupLinkedAsync(id);

            result.Should().BeTrue();
        }

        // --- GetAllItemGroupsAsync ---

        [Fact]
        public async Task GetAllItemGroupsAsync_Should_Return_NonDeleted()
        {
            await ClearAsync();
            await SeedAsync("A1");
            await SeedAsync("A2");
            await SeedAsync("ADEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllItemGroupsAsync();

            result.Should().HaveCount(2);
        }

        // --- Role filter: restricted access ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_RoleFilter_Excludes_Id()
        {
            await ClearAsync();
            var id = await SeedAsync("RF1");
            var dataAccess = new Mock<IDataAccessFilter>(MockBehavior.Loose);
            dataAccess.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataAccessContext
                {
                    BypassDataAccess = false,
                    AllowedItemGroupIds = new HashSet<int> { id + 999 } // different from seeded
                });

            var result = await CreateRepo(dataAccess: dataAccess).GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
