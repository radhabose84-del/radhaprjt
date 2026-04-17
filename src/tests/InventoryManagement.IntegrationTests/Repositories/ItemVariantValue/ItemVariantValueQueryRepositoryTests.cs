using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemVariantValue
{
    [Collection("DatabaseCollection")]
    public sealed class ItemVariantValueQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantValueQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemVariantValueQueryRepository CreateRepo() =>
            new(_fixture.CreateFreshDbContext());

        private async Task<int> SeedItemAsync(string code, string name, int? parentItemId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var item = new ItemMaster
            {
                ItemCode = code,
                ItemName = name,
                ParentItemId = parentItemId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemMaster.AddAsync(item);
            await ctx.SaveChangesAsync();
            return item.Id;
        }

        private async Task<int> SeedSpecMasterAsync(string name)
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

        private async Task<int> SeedSpecValueAsync(int specMasterId, string value)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = specMasterId,
                SpecificationValue = value,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationValue.AddAsync(v);
            await ctx.SaveChangesAsync();
            return v.Id;
        }

        private async Task<int> SeedVariantAttributeAsync(int itemId, int specMasterId, int order)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var a = new ItemVariantAttribute
            {
                ItemId = itemId,
                SpecificationMasterId = specMasterId,
                Order = order
            };
            await ctx.ItemVariantAttribute.AddAsync(a);
            await ctx.SaveChangesAsync();
            return a.Id;
        }

        private async Task SeedVariantValueAsync(int itemId, int variantAttrId, int specValueId, int parentItemId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemVariantValue
            {
                ItemId = itemId,
                VariantAttributeId = variantAttrId,
                SpecificationValueId = specValueId,
                ParentItemId = parentItemId
            };
            await ctx.ItemVariantValue.AddAsync(v);
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetForItemGroupedAsync ---

        [Fact]
        public async Task GetForItemGroupedAsync_Should_Return_Grouped_Values()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_G1", "Template");
            var childId = await SeedItemAsync("CH_G1", "Child", tmplId);
            var smColor = await SeedSpecMasterAsync("CG1xA");
            var smSize = await SeedSpecMasterAsync("CG1xB");
            var red = await SeedSpecValueAsync(smColor, "Red");
            var large = await SeedSpecValueAsync(smSize, "Large");
            var attrC = await SeedVariantAttributeAsync(childId, smColor, 1);
            var attrS = await SeedVariantAttributeAsync(childId, smSize, 2);
            await SeedVariantValueAsync(childId, attrC, red, tmplId);
            await SeedVariantValueAsync(childId, attrS, large, tmplId);

            var result = await CreateRepo().GetForItemGroupedAsync(childId, CancellationToken.None);

            result.Should().HaveCount(2);
            result[attrC].Should().ContainSingle().Which.Should().Be(red);
            result[attrS].Should().ContainSingle().Which.Should().Be(large);
        }

        [Fact]
        public async Task GetForItemGroupedAsync_Should_Return_Empty_When_NoValues()
        {
            await ClearAsync();
            var id = await SeedItemAsync("NV1", "NoValues");

            var result = await CreateRepo().GetForItemGroupedAsync(id, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetForItemGroupedAsync_Should_Deduplicate_SpecValueIds()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_DDUP", "Template");
            var childId = await SeedItemAsync("CH_DDUP", "Child", tmplId);
            var smA = await SeedSpecMasterAsync("DdupA");
            var smB = await SeedSpecMasterAsync("DdupB");
            var valA = await SeedSpecValueAsync(smA, "A");
            var valB = await SeedSpecValueAsync(smB, "B");
            var attrA = await SeedVariantAttributeAsync(childId, smA, 1);
            var attrB = await SeedVariantAttributeAsync(childId, smB, 2);
            await SeedVariantValueAsync(childId, attrA, valA, tmplId);
            await SeedVariantValueAsync(childId, attrB, valB, tmplId);

            var result = await CreateRepo().GetForItemGroupedAsync(childId, CancellationToken.None);

            result.Should().HaveCount(2);
            result[attrA].Should().ContainSingle().Which.Should().Be(valA);
            result[attrB].Should().ContainSingle().Which.Should().Be(valB);
        }

        // --- GetExistingChildComboKeysAsync ---

        [Fact]
        public async Task GetExistingChildComboKeysAsync_Should_Return_Keys_For_Children()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_CK", "TemplateCK");
            var child1 = await SeedItemAsync("C1_CK", "Child1", tmplId);
            var child2 = await SeedItemAsync("C2_CK", "Child2", tmplId);
            var sm = await SeedSpecMasterAsync("SizeCK");
            var small = await SeedSpecValueAsync(sm, "S");
            var large = await SeedSpecValueAsync(sm, "L");
            var attrTmpl = await SeedVariantAttributeAsync(tmplId, sm, 1);
            await SeedVariantValueAsync(child1, attrTmpl, small, tmplId);
            await SeedVariantValueAsync(child2, attrTmpl, large, tmplId);

            var keys = await CreateRepo().GetExistingChildComboKeysAsync(tmplId, CancellationToken.None);

            keys.Should().HaveCount(2);
            keys.Should().Contain($"{attrTmpl}:{small}");
            keys.Should().Contain($"{attrTmpl}:{large}");
        }

        [Fact]
        public async Task GetExistingChildComboKeysAsync_Should_Return_Empty_When_NoChildren()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_NOCH", "TemplateNoCh");

            var keys = await CreateRepo().GetExistingChildComboKeysAsync(tmplId, CancellationToken.None);

            keys.Should().BeEmpty();
        }

        // --- GetExistingChildCombosWithIdsAsync ---

        [Fact]
        public async Task GetExistingChildCombosWithIdsAsync_Should_Return_Combo_To_ChildId_Map()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_MAP", "TmplMap");
            var child1 = await SeedItemAsync("C1_MAP", "Child1", tmplId);
            var child2 = await SeedItemAsync("C2_MAP", "Child2", tmplId);
            var sm = await SeedSpecMasterAsync("ColorMap");
            var red = await SeedSpecValueAsync(sm, "Red");
            var blue = await SeedSpecValueAsync(sm, "Blue");
            var attrTmpl = await SeedVariantAttributeAsync(tmplId, sm, 1);
            await SeedVariantValueAsync(child1, attrTmpl, red, tmplId);
            await SeedVariantValueAsync(child2, attrTmpl, blue, tmplId);

            var map = await CreateRepo().GetExistingChildCombosWithIdsAsync(tmplId, CancellationToken.None);

            map.Should().HaveCount(2);
            map[$"{attrTmpl}:{red}"].Should().Be(child1);
            map[$"{attrTmpl}:{blue}"].Should().Be(child2);
        }

        [Fact]
        public async Task GetExistingChildCombosWithIdsAsync_Should_Return_Empty_When_NoAttributes()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_NOAT", "NoAttrs");

            var map = await CreateRepo().GetExistingChildCombosWithIdsAsync(tmplId, CancellationToken.None);

            map.Should().BeEmpty();
        }

        [Fact]
        public async Task GetExistingChildCombosWithIdsAsync_Should_Skip_Partial_Combos()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_PART", "TmplPart");
            var child = await SeedItemAsync("C_PART", "Child", tmplId);
            var smColor = await SeedSpecMasterAsync("ColorPart");
            var smSize = await SeedSpecMasterAsync("SizePart");
            var red = await SeedSpecValueAsync(smColor, "Red");
            var _ = await SeedSpecValueAsync(smSize, "L");
            var attrColor = await SeedVariantAttributeAsync(tmplId, smColor, 1);
            await SeedVariantAttributeAsync(tmplId, smSize, 2); // template has 2 attrs
            await SeedVariantValueAsync(child, attrColor, red, tmplId); // child only has 1 value

            var map = await CreateRepo().GetExistingChildCombosWithIdsAsync(tmplId, CancellationToken.None);

            map.Should().BeEmpty();
        }

        // --- GetForItemAsync ---

        [Fact]
        public async Task GetForItemAsync_Should_Return_VariantValueDtos()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("TMPL_DTO", "Template");
            var childId = await SeedItemAsync("C_DTO", "Child", tmplId);
            var sm = await SeedSpecMasterAsync("ColorDto");
            var red = await SeedSpecValueAsync(sm, "RedDto");
            var attr = await SeedVariantAttributeAsync(childId, sm, 1);
            await SeedVariantValueAsync(childId, attr, red, tmplId);

            var result = await CreateRepo().GetForItemAsync(childId, CancellationToken.None);

            result.Should().ContainSingle();
            result[0].VariantAttributeId.Should().Be(attr);
            result[0].SpecificationValueId.Should().Be(red);
            result[0].SpecificationValue.Should().Be("RedDto");
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Empty_When_NoValues()
        {
            await ClearAsync();
            var id = await SeedItemAsync("NOVD", "NoVals");

            var result = await CreateRepo().GetForItemAsync(id, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
