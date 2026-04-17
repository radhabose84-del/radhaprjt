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
    public sealed class BudgetDepartmentValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetDepartmentValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetDepartmentValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BudgetDepartmentValidationRepository(conn);
        }

        private async Task SeedBudgetGroupAsync(
            int departmentId,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Name = "GrpD" + departmentId, Description = "d",
                UnitId = 1, DepartmentId = departmentId, CostCenterId = 1, CurrencyId = 1,
                IsActive = active,
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

            var grp = await ctx.BudgetGroups.ToListAsync();
            ctx.BudgetGroups.RemoveRange(grp);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedDepartmentAsync ---

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_Group_Uses_DepartmentId()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(departmentId: 10);

            var result = await CreateRepo().HasLinkedDepartmentAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_Unused()
        {
            await ClearAsync();

            var result = await CreateRepo().HasLinkedDepartmentAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(departmentId: 20, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedDepartmentAsync(20);

            result.Should().BeFalse();
        }

        // --- HasActiveDepartmentAsync ---

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_True_When_Group_Is_Active()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(departmentId: 30, active: BaseEntity.Status.Active);

            var result = await CreateRepo().HasActiveDepartmentAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_Group_Is_Inactive()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(departmentId: 40, active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().HasActiveDepartmentAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_Group_SoftDeleted()
        {
            await ClearAsync();
            await SeedBudgetGroupAsync(departmentId: 50, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().HasActiveDepartmentAsync(50);

            result.Should().BeFalse();
        }
    }
}
