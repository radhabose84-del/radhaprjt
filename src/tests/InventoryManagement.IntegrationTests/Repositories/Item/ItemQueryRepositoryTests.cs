using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.ProductionManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.SalesManagement;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Item
{
    [Collection("DatabaseCollection")]
    public sealed class ItemQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemQueryRepository CreateRepo(
            Mock<ISalesItemValidation>? sales = null,
            Mock<IPurchaseItemValidation>? purchase = null,
            Mock<IMaintenanceItemValidation>? maintenance = null,
            Mock<IProductionItemValidation>? production = null,
            Mock<IDataAccessFilter>? dataAccess = null)
        {
            if (sales == null)
            {
                sales = new Mock<ISalesItemValidation>(MockBehavior.Loose);
                sales.Setup(s => s.HasLinkedItemAsync(It.IsAny<int>())).ReturnsAsync(false);
                sales.Setup(s => s.HasActiveItemAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (purchase == null)
            {
                purchase = new Mock<IPurchaseItemValidation>(MockBehavior.Loose);
                purchase.Setup(p => p.HasLinkedItemAsync(It.IsAny<int>())).ReturnsAsync(false);
                purchase.Setup(p => p.HasActiveItemAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (maintenance == null)
            {
                maintenance = new Mock<IMaintenanceItemValidation>(MockBehavior.Loose);
                maintenance.Setup(m => m.HasLinkedItemAsync(It.IsAny<int>())).ReturnsAsync(false);
                maintenance.Setup(m => m.HasActiveItemAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (production == null)
            {
                production = new Mock<IProductionItemValidation>(MockBehavior.Loose);
                production.Setup(p => p.HasLinkedItemAsync(It.IsAny<int>())).ReturnsAsync(false);
                production.Setup(p => p.HasActiveItemAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (dataAccess == null)
            {
                dataAccess = new Mock<IDataAccessFilter>(MockBehavior.Loose);
                dataAccess.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(DataAccessContext.Unrestricted);
            }

            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            var countLookup = new Mock<ICountMasterLookup>(MockBehavior.Loose);
            var rawLookup = new Mock<IRawMaterialTypeLookup>(MockBehavior.Loose);
            var salesGrpLookup = new Mock<ISalesGroupLookup>(MockBehavior.Loose);

            var conn = new SqlConnection(_fixture.ConnectionString);
            var dbCtx = _fixture.CreateFreshDbContext();
            return new ItemQueryRepository(conn, dbCtx, _fixture.IpMock.Object,
                unitLookup.Object, countLookup.Object, rawLookup.Object, salesGrpLookup.Object,
                dataAccess.Object,
                sales.Object, purchase.Object, maintenance.Object, production.Object);
        }

        private async Task<int> SeedItemAsync(
            string code,
            string? name = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted,
            int? parentItemId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var i = new ItemMaster
            {
                ItemCode = code,
                ItemName = name ?? $"Item {code}",
                IsActive = active,
                IsDeleted = deleted,
                ParentItemId = parentItemId,
                IsOnSpot = false
            };
            await ctx.ItemMaster.AddAsync(i);
            await ctx.SaveChangesAsync();
            return i.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedItemAsync("QI1");

            var (items, total) = await CreateRepo().GetAllAsync(1, 10, null!, false, null, null);

            items.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedItemAsync("QIDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, "QIDEL", false, null, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_OnlyActive_Flag()
        {
            await ClearAsync();
            await SeedItemAsync("QIACT1");
            await SeedItemAsync("QIIN1", active: Status.Inactive);

            var (items, _) = await CreateRepo().GetAllAsync(1, 10, null!, true, null, null);

            items.Should().OnlyContain(i => i.ItemCode != "QIIN1");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_RoleFilter_Denies_All()
        {
            await ClearAsync();
            await SeedItemAsync("QROL");
            var dataAccess = new Mock<IDataAccessFilter>(MockBehavior.Loose);
            dataAccess.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataAccessContext
                {
                    BypassDataAccess = false,
                    AllowedItemGroupIds = new HashSet<int>()
                });

            var (items, total) = await CreateRepo(dataAccess: dataAccess).GetAllAsync(1, 10, null!, false, null, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedItemAsync("SDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Has_Child_Item()
        {
            await ClearAsync();
            var parentId = await SeedItemAsync("SDV_P");
            await SeedItemAsync("SDV_C", parentItemId: parentId);

            var result = await CreateRepo().SoftDeleteValidationAsync(parentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearAsync();
            var id = await SeedItemAsync("SDV_SAL");
            var sales = new Mock<ISalesItemValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedItemAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(sales: sales).SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        // --- IsItemMasterLinkedAsync ---

        [Fact]
        public async Task IsItemMasterLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedItemAsync("LK1");

            var result = await CreateRepo().IsItemMasterLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsItemMasterLinkedAsync_Should_Return_True_When_PurchaseValidation_Active()
        {
            await ClearAsync();
            var id = await SeedItemAsync("LK2");
            var purchase = new Mock<IPurchaseItemValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasActiveItemAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(purchase: purchase).IsItemMasterLinkedAsync(id);

            result.Should().BeTrue();
        }

        // --- GetCandidateItemNamesAsync ---

        [Fact]
        public async Task GetCandidateItemNamesAsync_Should_Return_Matching()
        {
            await ClearAsync();
            await SeedItemAsync("CN1", "UniqueCandidate");

            var result = await CreateRepo().GetCandidateItemNamesAsync("UniqueCandidate");

            result.Should().Contain("UniqueCandidate");
        }

        // --- GetBaseDirectoryAsync ---

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Return_Value_When_MiscType_Seeded()
        {
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "ItemImage");
                if (existing == null)
                {
                    await ctx.MiscTypeMaster.AddAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
                    {
                        MiscTypeCode = "ItemImage",
                        Description = "/uploads/items",
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    });
                    await ctx.SaveChangesAsync();
                }
            }

            var result = await CreateRepo().GetBaseDirectoryAsync();

            result.Should().Be("/uploads/items");
        }
    }
}
