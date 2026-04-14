using Infrastructure.Persistence.Repositories;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ItemGroupEntity = InventoryManagement.Domain.Entities.Item.ItemGroup;
using ItemCategoryEntity = InventoryManagement.Domain.Entities.Item.ItemCategory;

namespace InventoryManagement.IntegrationTests.Repositories.Budget
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetLog]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetDetail]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[BudgetMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[PutAwayRule]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategoryModule]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemCategory]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        private async Task<int> SeedCategoryAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new ItemGroupEntity
            {
                UnitId = 1, ItemGroupCode = "BGRP_CMD", ItemGroupName = "Cmd Budget Group",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemGroup.Add(group);
            await ctx.SaveChangesAsync();

            var category = new ItemCategoryEntity
            {
                ItemGroupId = group.Id, ItemCategoryName = "Cmd Budget Category",
                IsBudgetApplicable = (byte?)1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.ItemCategory.Add(category);
            await ctx.SaveChangesAsync();

            return category.Id;
        }

        private async Task<int> SeedBudgetDirectlyAsync(int categoryId, int fiscalYear = 2025)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var budget = new BudgetMaster
            {
                UnitId = 1,
                BudgetGroupId = categoryId,
                FiscalYear = fiscalYear,
                YearBudgetAmount = 200000m,
                Is_MRApplicable = (byte?)1,
                Is_POApplicable = (byte?)0,
                Is_ServiceApplicable = (byte?)0,
                BudgetDetail = new List<BudgetDetail>()
            };
            ctx.BudgetMaster.Add(budget);
            await ctx.SaveChangesAsync();
            return budget.Id;
        }

        // --- ExistsAsync ---

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_NoData()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).ExistsAsync(9999, 2099);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_BudgetExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var categoryId = await SeedCategoryAsync();
            await SeedBudgetDirectlyAsync(categoryId, 2025);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).ExistsAsync(categoryId, 2025);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_For_DifferentFiscalYear()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var categoryId = await SeedCategoryAsync();
            await SeedBudgetDirectlyAsync(categoryId, 2025);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).ExistsAsync(categoryId, 2026);

            result.Should().BeFalse();
        }

        // --- CreateBudgetAsync (no details — skips MiscMaster log requirement) ---

        [Fact]
        public async Task CreateBudgetAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var categoryId = await SeedCategoryAsync();

            var budget = new BudgetMaster
            {
                BudgetGroupId = categoryId,
                FiscalYear = 2030,
                YearBudgetAmount = 150000m,
                Is_MRApplicable = (byte?)1,
                Is_POApplicable = (byte?)0,
                Is_ServiceApplicable = (byte?)0,
                BudgetDetail = new List<BudgetDetail>()   // empty — avoids MiscMaster lookup
            };

            var id = await CreateRepository(ctx).CreateBudgetAsync(budget);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateBudgetAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var categoryId = await SeedCategoryAsync();

            var budget = new BudgetMaster
            {
                BudgetGroupId = categoryId,
                FiscalYear = 2031,
                YearBudgetAmount = 75000m,
                Is_MRApplicable = (byte?)1,
                Is_POApplicable = (byte?)0,
                Is_ServiceApplicable = (byte?)0,
                BudgetDetail = new List<BudgetDetail>()
            };

            var id = await CreateRepository(ctx).CreateBudgetAsync(budget);
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var saved = await ctx2.BudgetMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.BudgetGroupId.Should().Be(categoryId);
            saved.FiscalYear.Should().Be(2031);
            saved.YearBudgetAmount.Should().Be(75000m);
            saved.UnitId.Should().Be(1);
        }

        [Fact]
        public async Task CreateBudgetAsync_Then_ExistsAsync_Should_Return_True()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var categoryId = await SeedCategoryAsync();

            var budget = new BudgetMaster
            {
                BudgetGroupId = categoryId,
                FiscalYear = 2032,
                YearBudgetAmount = 100000m,
                Is_MRApplicable = (byte?)1,
                Is_POApplicable = (byte?)0,
                Is_ServiceApplicable = (byte?)0,
                BudgetDetail = new List<BudgetDetail>()
            };

            await CreateRepository(ctx).CreateBudgetAsync(budget);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var exists = await CreateRepository(ctx2).ExistsAsync(categoryId, 2032);

            exists.Should().BeTrue();
        }
    }
}
