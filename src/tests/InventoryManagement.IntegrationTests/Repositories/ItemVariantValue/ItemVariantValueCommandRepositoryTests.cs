using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemVariantValue
{
    [Collection("DatabaseCollection")]
    public sealed class ItemVariantValueCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantValueCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemVariantValueCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedItemAsync(string code, int? parentId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var i = new ItemMaster
            {
                ItemCode = code, ItemName = $"Item {code}",
                ParentItemId = parentId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemMaster.AddAsync(i);
            await ctx.SaveChangesAsync();
            return i.Id;
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
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedSpecValueAsync(int smId, string value)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = smId, SpecificationValue = value,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationValue.AddAsync(v);
            await ctx.SaveChangesAsync();
            return v.Id;
        }

        private async Task<int> SeedVariantAttrAsync(int itemId, int smId, int order)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var a = new Domain.Entities.Item.ItemDetail.Variant.ItemVariantAttribute
            {
                ItemId = itemId, SpecificationMasterId = smId, Order = order
            };
            await ctx.ItemVariantAttribute.AddAsync(a);
            await ctx.SaveChangesAsync();
            return a.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetForItemAsync_Should_Return_Empty_When_None()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVV_G1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetForItemAsync(itemId, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpsertListAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("IVV_T1");
            var childId = await SeedItemAsync("IVV_C1", parentId: tmplId);
            var smId = await SeedSpecMasterAsync("IVV_SM1");
            var svId = await SeedSpecValueAsync(smId, "R");
            var attrId = await SeedVariantAttrAsync(tmplId, smId, 1);
            await using var ctx = _fixture.CreateFreshDbContext();

            var values = new List<VariantValueDto>
            {
                new() { VariantAttributeId = attrId, SpecificationValueId = svId }
            };
            await CreateRepo(ctx).UpsertListAsync(childId, values, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemVariantValue.Where(x => x.ItemId == childId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].ParentItemId.Should().Be(tmplId);
        }

        [Fact]
        public async Task UpsertListAsync_Should_Throw_When_Item_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).UpsertListAsync(
                9999999, new List<VariantValueDto>(), CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task UpsertListAsync_Should_Throw_When_AttrId_Not_Belonging_To_Template()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("IVV_T2");
            var otherTmplId = await SeedItemAsync("IVV_TOTHER");
            var childId = await SeedItemAsync("IVV_C2", parentId: tmplId);
            var smId = await SeedSpecMasterAsync("IVV_SM2");
            var svId = await SeedSpecValueAsync(smId, "Foo");
            // Attr belongs to otherTmplId, NOT tmplId
            var badAttrId = await SeedVariantAttrAsync(otherTmplId, smId, 1);

            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).UpsertListAsync(childId, new List<VariantValueDto>
            {
                new() { VariantAttributeId = badAttrId, SpecificationValueId = svId }
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*do not belong to template*");
        }

        [Fact]
        public async Task UpsertListAsync_Should_Update_Existing_SpecValueId()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("IVV_T3");
            var childId = await SeedItemAsync("IVV_C3", parentId: tmplId);
            var smId = await SeedSpecMasterAsync("IVV_SM3");
            var sv1 = await SeedSpecValueAsync(smId, "V1");
            var sv2 = await SeedSpecValueAsync(smId, "V2");
            var attrId = await SeedVariantAttrAsync(tmplId, smId, 1);
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemVariantValue.Add(new Domain.Entities.Item.ItemDetail.Variant.ItemVariantValue
                {
                    ItemId = childId, ParentItemId = tmplId,
                    VariantAttributeId = attrId, SpecificationValueId = sv1
                });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpsertListAsync(childId, new List<VariantValueDto>
            {
                new() { VariantAttributeId = attrId, SpecificationValueId = sv2 }
            }, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemVariantValue.AsNoTracking().FirstAsync(x => x.ItemId == childId);
            reloaded.SpecificationValueId.Should().Be(sv2);
        }

        [Fact]
        public async Task UpsertListAsync_Empty_Should_Remove_All_For_Item()
        {
            await ClearAsync();
            var tmplId = await SeedItemAsync("IVV_T4");
            var childId = await SeedItemAsync("IVV_C4", parentId: tmplId);
            var smId = await SeedSpecMasterAsync("IVV_SM4");
            var svId = await SeedSpecValueAsync(smId, "X");
            var attrId = await SeedVariantAttrAsync(tmplId, smId, 1);
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemVariantValue.Add(new Domain.Entities.Item.ItemDetail.Variant.ItemVariantValue
                {
                    ItemId = childId, ParentItemId = tmplId,
                    VariantAttributeId = attrId, SpecificationValueId = svId
                });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpsertListAsync(childId, new List<VariantValueDto>(), CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var saved = await ctx2.ItemVariantValue.AsNoTracking().Where(x => x.ItemId == childId).ToListAsync();
            saved.Should().BeEmpty();
        }
    }
}
