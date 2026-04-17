using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using BudgetManagement.Infrastructure.Repositories.Validations;
using BudgetManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BudgetManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetCurrencyValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetCurrencyValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetCurrencyValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BudgetCurrencyValidationRepository(conn);
        }

        private async Task SeedBudgetGroupAsync(int currencyId, BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Name = "Grp" + currencyId, Description = "d",
                UnitId = 1, DepartmentId = 1, CostCenterId = 1,
                CurrencyId = currencyId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = deleted
            };
            await ctx.BudgetGroups.AddAsync(g);
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var allocs = await ctx.BudgetAllocations.ToListAsync();
            ctx.BudgetAllocations.RemoveRange(allocs);
            await ctx.SaveChangesAsync();

            var br = await ctx.BudgetRequests.ToListAsync();
            ctx.BudgetRequests.RemoveRange(br);
            await ctx.SaveChangesAsync();

            var grp = await ctx.BudgetGroups.ToListAsync();
            ctx.BudgetGroups.RemoveRange(grp);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_True_When_BudgetGroup_References_CurrencyId()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(currencyId: 42);

            var result = await CreateRepo().HasLinkedCurrencyAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_CurrencyId_Unused()
        {
            await ClearAsync();

            var result = await CreateRepo().HasLinkedCurrencyAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_Group_SoftDeleted()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(currencyId: 77, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedCurrencyAsync(77);

            result.Should().BeFalse();
        }
    }
}
