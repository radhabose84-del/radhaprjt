using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemVariantAttributeTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemVariantAttributeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemVariantAttributeCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemVariantAttributeCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedItemAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var i = new ItemMaster
            {
                ItemCode = code, ItemName = $"Item {code}",
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

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task UpsertAttributesAsync_Should_Insert_New_Rows()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVA_U1");
            var smId = await SeedSpecMasterAsync("IVA_S1");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpsertAttributesAsync(itemId, new List<VariantAttributeDto>
            {
                new() { SpecificationMasterId = smId, Order = 1 }
            }, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemVariantAttribute.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().ContainSingle();
            saved[0].SpecificationMasterId.Should().Be(smId);
        }

        [Fact]
        public async Task UpsertAttributesAsync_Should_Update_Existing_Order()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVA_U2");
            var smId = await SeedSpecMasterAsync("IVA_S2");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemVariantAttribute.Add(new Domain.Entities.Item.ItemDetail.Variant.ItemVariantAttribute
                {
                    ItemId = itemId, SpecificationMasterId = smId, Order = 1
                });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx2).UpsertAttributesAsync(itemId, new List<VariantAttributeDto>
            {
                new() { SpecificationMasterId = smId, Order = 5 }
            }, CancellationToken.None);
            await ctx2.SaveChangesAsync();

            var reloaded = await ctx2.ItemVariantAttribute.AsNoTracking()
                .FirstAsync(x => x.ItemId == itemId && x.SpecificationMasterId == smId);
            reloaded.Order.Should().Be(5);
        }

        [Fact]
        public async Task UpsertAttributesAsync_Should_Throw_When_SpecMaster_Missing()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVA_GHOST");
            await using var ctx = _fixture.CreateFreshDbContext();

            Func<Task> act = async () => await CreateRepo(ctx).UpsertAttributesAsync(itemId, new List<VariantAttributeDto>
            {
                new() { SpecificationMasterId = 9999999, Order = 1 }
            }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid ItemSpecificationMaster*");
        }

        [Fact]
        public async Task UpsertAttributesAsync_Should_Ignore_Empty_Or_Invalid()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVA_INV");
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).UpsertAttributesAsync(itemId, new List<VariantAttributeDto>
            {
                new() { SpecificationMasterId = 0, Order = 1 }
            }, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var saved = await ctx.ItemVariantAttribute.Where(x => x.ItemId == itemId).ToListAsync();
            saved.Should().BeEmpty();
        }

        [Fact]
        public async Task GetForItemAsync_Should_Return_Seeded_Dtos()
        {
            await ClearAsync();
            var itemId = await SeedItemAsync("IVA_G1");
            var smId = await SeedSpecMasterAsync("IVA_GS1");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.ItemVariantAttribute.Add(new Domain.Entities.Item.ItemDetail.Variant.ItemVariantAttribute
                {
                    ItemId = itemId, SpecificationMasterId = smId, Order = 2
                });
                await ctx.SaveChangesAsync();
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetForItemAsync(itemId, CancellationToken.None);

            result.Should().ContainSingle();
            result[0].SpecificationMasterId.Should().Be(smId);
            result[0].Order.Should().Be(2);
        }

        [Fact]
        public async Task GetSpecificationValueMapAsync_Should_Return_Empty_For_Empty_Input()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetSpecificationValueMapAsync(Array.Empty<int>(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSpecificationValueMapAsync_Should_Return_Values()
        {
            await ClearAsync();
            var smId = await SeedSpecMasterAsync("IVA_MAP1");
            await using var ctx = _fixture.CreateFreshDbContext();
            var v = new Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = smId, SpecificationValue = "Red",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationValue.AddAsync(v);
            await ctx.SaveChangesAsync();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx2).GetSpecificationValueMapAsync(new[] { v.Id }, CancellationToken.None);

            result.Should().ContainKey(v.Id);
            result[v.Id].SpecMasterId.Should().Be(smId);
            result[v.Id].SpecValueName.Should().Be("Red");
        }
    }
}
