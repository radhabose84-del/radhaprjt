using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.PurchaseManagement;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Item;
using InventoryManagement.Infrastructure.Repositories.Item.ItemCategory;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemCategoryTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemCategoryUnitConfigRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemCategoryUnitConfigRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemCategoryCommandRepository CreateCommandRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private ItemCategoryQueryRepository CreateQueryRepo()
        {
            var purchase = new Mock<IPurchaseItemCategoryValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasLinkedItemCategoryAsync(It.IsAny<int>())).ReturnsAsync(false);
            purchase.Setup(p => p.HasActiveItemCategoryAsync(It.IsAny<int>())).ReturnsAsync(false);

            var module = new Mock<IModuleLookup>(MockBehavior.Loose);
            module.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(new List<ModuleLookupDto>
                {
                    new() { ModuleId = 1, ModuleName = "Module 1" }
                });

            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new UnitLookupDto { UnitId = id, UnitName = $"Unit-{id}" }).ToList());
            unit.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new UnitLookupDto { UnitId = id, UnitName = $"Unit-{id}" });

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ItemCategoryQueryRepository(conn, purchase.Object, module.Object, unit.Object);
        }

        private async Task<int> EnsureItemGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemGroup.FirstOrDefaultAsync(g => g.ItemGroupCode == "ICUC_GRP");
            if (existing != null) return existing.Id;
            var g = new ItemGroup
            {
                UnitId = 1, ItemGroupCode = "ICUC_GRP", ItemGroupName = "Unit Config Test Grp",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<int> EnsureUomTypeMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "ICUC_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ICUC_T", Description = "UC UOM Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "ICUCMM");
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = "ICUCMM", Description = "UC UOM Misc",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<int> EnsureUOMAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.UOMs.FirstOrDefaultAsync(u => u.Code == "ICUCKG");
            if (existing != null) return existing.Id;
            var typeId = await EnsureUomTypeMiscAsync();
            var u = new UOM
            {
                Code = "ICUCKG", UOMName = "Kilogram-Test",
                UOMTypeId = typeId, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.UOMs.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        private async Task<InventoryManagement.Domain.Entities.Item.ItemCategory> BuildCategoryAsync(string name = "UC_Cat1")
        {
            var groupId = await EnsureItemGroupAsync();
            return new InventoryManagement.Domain.Entities.Item.ItemCategory
            {
                ItemGroupId = groupId,
                ItemCategoryName = name,
                IsGroup = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // ---------- Create ----------

        [Fact]
        public async Task CreateAsync_With_UnitConfigs_ShouldPersistRows()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var configs = new List<ItemCategoryUnitConfig>
            {
                new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 10m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                new() { UnitId = 2, UOMId = uomId, MaxSampleQuantity = 25m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };

            var catId = await CreateCommandRepo(ctx).CreateAsync(
                await BuildCategoryAsync("UC_Create1"), new List<int>(), configs);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemCategoryUnitConfig.Where(c => c.ItemCategoryId == catId).ToListAsync();
            saved.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_With_DuplicateUnitId_Dedups()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var configs = new List<ItemCategoryUnitConfig>
            {
                new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 5m,  IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 99m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };

            var catId = await CreateCommandRepo(ctx).CreateAsync(
                await BuildCategoryAsync("UC_Dedup"), new List<int>(), configs);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemCategoryUnitConfig.Where(c => c.ItemCategoryId == catId).ToListAsync();
            saved.Should().HaveCount(1);
        }

        // ---------- Update (diff strategy) ----------

        [Fact]
        public async Task UpdateAsync_AddsNewConfigs_UpdatesExisting_SoftDeletesMissing()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            await using (var seedCtx = _fixture.CreateFreshDbContext())
            {
                var initialConfigs = new List<ItemCategoryUnitConfig>
                {
                    new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 10m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                    new() { UnitId = 2, UOMId = uomId, MaxSampleQuantity = 20m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
                };
                await CreateCommandRepo(seedCtx).CreateAsync(
                    await BuildCategoryAsync("UC_DiffStrategy"), new List<int>(), initialConfigs);
            }

            int catId;
            int existingUnit1Id;
            await using (var readCtx = _fixture.CreateFreshDbContext())
            {
                var cat = await readCtx.ItemCategory.FirstAsync(c => c.ItemCategoryName == "UC_DiffStrategy");
                catId = cat.Id;
                var existing = await readCtx.ItemCategoryUnitConfig
                    .Where(c => c.ItemCategoryId == catId && c.UnitId == 1)
                    .FirstAsync();
                existingUnit1Id = existing.Id;
            }

            await using (var updateCtx = _fixture.CreateFreshDbContext())
            {
                var newPayload = new List<ItemCategoryUnitConfig>
                {
                    new() { Id = existingUnit1Id, UnitId = 1, UOMId = uomId, MaxSampleQuantity = 50m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                    new() { Id = 0, UnitId = 3, UOMId = uomId, MaxSampleQuantity = 30m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
                };
                await CreateCommandRepo(updateCtx).UpdateAsync(catId, await BuildCategoryAsync("UC_DiffStrategy"), new List<int>(), newPayload);
            }

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var allRows = await verifyCtx.ItemCategoryUnitConfig.Where(c => c.ItemCategoryId == catId).ToListAsync();
            var live = allRows.Where(r => r.IsDeleted == IsDelete.NotDeleted).ToList();
            var deleted = allRows.Where(r => r.IsDeleted == IsDelete.Deleted).ToList();

            live.Should().HaveCount(2);
            live.First(r => r.UnitId == 1).MaxSampleQuantity.Should().Be(50m);
            live.Any(r => r.UnitId == 3).Should().BeTrue();
            deleted.Should().HaveCount(1).And.Subject.First().UnitId.Should().Be(2);
        }

        // ---------- Delete cascade ----------

        [Fact]
        public async Task DeleteAsync_ParentSoftDelete_CascadesToUnitConfigs()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var configs = new List<ItemCategoryUnitConfig>
            {
                new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 10m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            };
            var catId = await CreateCommandRepo(ctx).CreateAsync(
                await BuildCategoryAsync("UC_Del"), new List<int>(), configs);
            ctx.ChangeTracker.Clear();

            var deleteEntity = new InventoryManagement.Domain.Entities.Item.ItemCategory { IsDeleted = IsDelete.Deleted };
            await CreateCommandRepo(ctx).DeleteAsync(catId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var childRows = await ctx.ItemCategoryUnitConfig.Where(c => c.ItemCategoryId == catId).ToListAsync();
            childRows.Should().AllSatisfy(r => r.IsDeleted.Should().Be(IsDelete.Deleted));
        }

        // ---------- Query repo ----------

        [Fact]
        public async Task GetSampleQuantitiesAsync_ReturnsAllLiveRowsWithNames()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            int catId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var configs = new List<ItemCategoryUnitConfig>
                {
                    new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 5m,  IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                    new() { UnitId = 2, UOMId = uomId, MaxSampleQuantity = 10m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
                };
                catId = await CreateCommandRepo(ctx).CreateAsync(
                    await BuildCategoryAsync("UC_Q1"), new List<int>(), configs);
            }

            var rows = await CreateQueryRepo().GetSampleQuantitiesAsync(catId);

            rows.Should().HaveCount(2);
            rows.Should().AllSatisfy(r =>
            {
                r.UnitName.Should().NotBeNull();
                r.UOMName.Should().Be("Kilogram-Test");
            });
        }

        [Fact]
        public async Task GetSampleQuantityAsync_OnlyReturnsActiveAndNotDeleted()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            int catId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var configs = new List<ItemCategoryUnitConfig>
                {
                    new() { UnitId = 1, UOMId = uomId, MaxSampleQuantity = 5m,  IsActive = Status.Inactive, IsDeleted = IsDelete.NotDeleted }
                };
                catId = await CreateCommandRepo(ctx).CreateAsync(
                    await BuildCategoryAsync("UC_Q2"), new List<int>(), configs);
            }

            var dto = await CreateQueryRepo().GetSampleQuantityAsync(catId, 1);
            dto.Should().BeNull();
        }

        [Fact]
        public async Task UnitExistsForCategoryAsync_TrueWhenLiveRowMatches()
        {
            await ClearAsync();
            var uomId = await EnsureUOMAsync();

            int catId;
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var configs = new List<ItemCategoryUnitConfig>
                {
                    new() { UnitId = 7, UOMId = uomId, MaxSampleQuantity = 1m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
                };
                catId = await CreateCommandRepo(ctx).CreateAsync(
                    await BuildCategoryAsync("UC_Q3"), new List<int>(), configs);
            }

            (await CreateQueryRepo().UnitExistsForCategoryAsync(catId, 7, null)).Should().BeTrue();
            (await CreateQueryRepo().UnitExistsForCategoryAsync(catId, 99, null)).Should().BeFalse();
        }
    }
}
