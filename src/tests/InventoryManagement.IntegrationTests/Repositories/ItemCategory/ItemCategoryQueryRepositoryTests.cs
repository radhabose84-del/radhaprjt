using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.PurchaseManagement;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemCategoryTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryQueryRepository CreateRepo(
            Mock<IPurchaseItemCategoryValidation>? purchase = null,
            Mock<IModuleLookup>? moduleLookup = null)
        {
            if (purchase == null)
            {
                purchase = new Mock<IPurchaseItemCategoryValidation>(MockBehavior.Loose);
                purchase.Setup(p => p.HasLinkedItemCategoryAsync(It.IsAny<int>())).ReturnsAsync(false);
                purchase.Setup(p => p.HasActiveItemCategoryAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (moduleLookup == null)
            {
                moduleLookup = new Mock<IModuleLookup>(MockBehavior.Loose);
                moduleLookup.Setup(m => m.GetAllModuleAsync())
                    .ReturnsAsync(new List<ModuleLookupDto>
                    {
                        new() { ModuleId = 1, ModuleName = "Module 1" },
                        new() { ModuleId = 2, ModuleName = "Module 2" }
                    });
            }

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemCategoryQueryRepository(conn, purchase.Object, moduleLookup.Object);
        }

        private async Task<int> EnsureItemGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemGroup.FirstOrDefaultAsync(g => g.ItemGroupCode == "ICQ_GRP");
            if (existing != null) return existing.Id;
            var g = new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                UnitId = 1, ItemGroupCode = "ICQ_GRP", ItemGroupName = "ItemCat Qry Grp",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<int> SeedAsync(
            string name,
            int? parentId = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted,
            byte isGroup = 0)
        {
            var groupId = await EnsureItemGroupAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var c = new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemGroupId = groupId,
                ItemCategoryName = name,
                IsGroup = isGroup,
                ParentCategoryId = parentId,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ItemCategory.AddAsync(c);
            await ctx.SaveChangesAsync();
            c.RootCategoryId = parentId ?? c.Id;
            ctx.ItemCategory.Update(c);
            await ctx.SaveChangesAsync();
            return c.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("QCat1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.ItemCategoryName.Should().Be("QCat1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("QCat_Del", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_Modules()
        {
            await ClearAsync();
            var id = await SeedAsync("QCat_Mod");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await ctx.ItemCategoryModule.AddAsync(new InventoryManagement.Domain.Entities.Item.ItemCategoryModule
                {
                    ItemCategoryId = id, ModuleId = 1
                });
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().GetByIdAsync(id);

            result.Modules.Should().HaveCount(1);
            result.Modules[0].ModuleName.Should().Be("Module 1");
        }

        // --- GetAllItemCategoryAsync ---

        [Fact]
        public async Task GetAllItemCategoryAsync_Should_Return_Seeded_Root()
        {
            await ClearAsync();
            await SeedAsync("QGA1");

            var (rows, total) = await CreateRepo().GetAllItemCategoryAsync(1, 10, null!, null);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllItemCategoryAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("QGA_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllItemCategoryAsync(1, 10, "QGA_DEL", null);

            total.Should().Be(0);
        }

        // --- SoftDeleteValidation ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedAsync("QSDV1");

            var result = await CreateRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_Has_Child_Category()
        {
            await ClearAsync();
            var rootId = await SeedAsync("QSDV_Root");
            await SeedAsync("QSDV_Child", parentId: rootId);

            var result = await CreateRepo().SoftDeleteValidation(rootId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_PurchaseValidation_Links()
        {
            await ClearAsync();
            var id = await SeedAsync("QSDV_Purch");
            var purchase = new Mock<IPurchaseItemCategoryValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasLinkedItemCategoryAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(purchase: purchase).SoftDeleteValidation(id);

            result.Should().BeTrue();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedAsync("NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        // --- GetItemCategoryAutoCompleteAsync ---

        [Fact]
        public async Task GetItemCategoryAutoCompleteAsync_Should_Return_Matching_Active()
        {
            await ClearAsync();
            await SeedAsync("AC_QAC1");
            await SeedAsync("AC_QAC2");

            var result = await CreateRepo().GetItemCategoryAutoCompleteAsync(
                null, "AC_QAC", isParent: false, excludeId: 0, moduleId: null);

            result.Should().HaveCount(2);
        }
    }
}
