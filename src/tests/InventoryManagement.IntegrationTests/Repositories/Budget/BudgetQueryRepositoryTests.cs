using Infrastructure.Persistence.Repositories;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Budget;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ItemGroupEntity = InventoryManagement.Domain.Entities.Item.ItemGroup;
using ItemCategoryEntity = InventoryManagement.Domain.Entities.Item.ItemCategory;

namespace InventoryManagement.IntegrationTests.Repositories.Budget
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetQueryRepository CreateQueryRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetLog]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetDetail]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        private async Task<(int groupId, int categoryId)> SeedGroupAndBudgetCategoryAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new ItemGroupEntity
            {
                UnitId = 1, ItemGroupCode = "BGRP001", ItemGroupName = "Budget Group",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemGroup.Add(group);
            await ctx.SaveChangesAsync();

            var category = new ItemCategoryEntity
            {
                ItemGroupId = group.Id, ItemCategoryName = "Budget Category",
                IsBudgetApplicable = (byte?)1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemCategory.Add(category);
            await ctx.SaveChangesAsync();

            return (group.Id, category.Id);
        }

        private async Task<int> SeedBudgetAsync(int categoryId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var budget = new BudgetMaster
            {
                UnitId = 1,
                BudgetGroupId = categoryId,
                FiscalYear = 2024,
                YearBudgetAmount = 100000,
                Is_MRApplicable = (byte?)1,
                Is_POApplicable = (byte?)0,
                Is_ServiceApplicable = (byte?)0,
                BudgetDetail = new List<BudgetDetail>()
            };
            ctx.BudgetMaster.Add(budget);
            await ctx.SaveChangesAsync();
            return budget.Id;
        }

        [Fact]
        public async Task GetBudgetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateQueryRepo(ctx).GetBudgetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBudgetByIdAsync_Should_Return_Dto_When_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, categoryId) = await SeedGroupAndBudgetCategoryAsync();
            var id = await SeedBudgetAsync(categoryId);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateQueryRepo(ctx2).GetBudgetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetAllBudgetsAsync_Should_Return_Empty_When_NoBudgets()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var items = await CreateQueryRepo(ctx).GetAllBudgetsAsync(null);

            items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllBudgetsAsync_Should_Return_Seeded_Budget()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (_, categoryId) = await SeedGroupAndBudgetCategoryAsync();
            await SeedBudgetAsync(categoryId);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var items = await CreateQueryRepo(ctx2).GetAllBudgetsAsync(null);

            items.Should().HaveCountGreaterThanOrEqualTo(1);
        }
    }
}
