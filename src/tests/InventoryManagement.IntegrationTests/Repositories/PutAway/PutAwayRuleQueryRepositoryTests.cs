using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Item.Templates;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ItemGroupEntity = InventoryManagement.Domain.Entities.Item.ItemGroup;
using ItemCategoryEntity = InventoryManagement.Domain.Entities.Item.ItemCategory;

namespace InventoryManagement.IntegrationTests.Repositories.PutAway
{
    [Collection("DatabaseCollection")]
    public sealed class PutAwayRuleQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PutAwayRuleQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PutAwayRuleQueryRepository CreateQueryRepo()
        {
            var rackLookup = new Mock<IRackLookup>(MockBehavior.Loose);
            rackLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<RackLookupDto>());

            var binLookup = new Mock<IBinLookup>(MockBehavior.Loose);
            binLookup.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BinLookupDto>());

            var whLookup = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            whLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<WarehouseLookupDto>());

            var ipService = _fixture.IpMock.Object;

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PutAwayRuleQueryRepository(conn, rackLookup.Object, binLookup.Object, whLookup.Object, ipService);
        }

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayStrategy]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        private async Task<(int groupId, int categoryId)> SeedGroupAndCategoryAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new ItemGroupEntity
            {
                UnitId = 1, ItemGroupCode = "QGRP001", ItemGroupName = "Query Group",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemGroup.Add(group);
            await ctx.SaveChangesAsync();

            var category = new ItemCategoryEntity
            {
                ItemGroupId = group.Id, ItemCategoryName = "Query Category",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemCategory.Add(category);
            await ctx.SaveChangesAsync();

            return (group.Id, category.Id);
        }

        private async Task<int> SeedRuleAsync(int groupId, int categoryId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var rule = new PutAwayRule
            {
                UnitId = 1, ItemGroupId = groupId, ItemCategoryId = categoryId,
                WarehouseId = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted,
                Strategies = new List<PutAwayStrategy>()
            };
            var repo = new PutAwayRuleCommandRepository(ctx);
            return await repo.CreateAsync(rule, CancellationToken.None);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Rule()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (groupId, categoryId) = await SeedGroupAndCategoryAsync();
            await SeedRuleAsync(groupId, categoryId);

            var (rows, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoRules()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (rows, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            rows.Should().BeEmpty();
            total.Should().Be(0);
        }
    }
}
