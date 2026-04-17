using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using BudgetManagement.Infrastructure.Repositories.Lookups.Budget;
using BudgetManagement.IntegrationTests.Common;
using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BudgetManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetGroupLookupRepository CreateRepo(int unitId = 1)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(s => s.GetUnitId()).Returns(unitId);
            return new BudgetGroupLookupRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedBudgetGroupAsync(
            string name,
            int unitId = 1,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Name = name,
                Description = name,
                UnitId = unitId,
                DepartmentId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.BudgetGroups.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var allocs = await ctx.BudgetAllocations.ToListAsync();
            ctx.BudgetAllocations.RemoveRange(allocs);
            await ctx.SaveChangesAsync();

            var rows = await ctx.BudgetGroups.ToListAsync();
            ctx.BudgetGroups.RemoveRange(rows);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Groups()
        {
            await ClearAsync();
            var id1 = await SeedBudgetGroupAsync("Alpha");
            var id2 = await SeedBudgetGroupAsync("Beta");
            await SeedBudgetGroupAsync("Gamma");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.Name).Should().Contain(new[] { "Alpha", "Beta" });
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await ClearAsync();
            var id = await SeedBudgetGroupAsync("Solo");

            var result = await CreateRepo().GetByIdsAsync(new[] { id, 0, -1 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            var id1 = await SeedBudgetGroupAsync("Active");
            var id2 = await SeedBudgetGroupAsync("Inactive", active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedBudgetGroupAsync("Keep");
            var id2 = await SeedBudgetGroupAsync("Drop", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Filter_By_UnitId()
        {
            await ClearAsync();
            var idUnit1 = await SeedBudgetGroupAsync("Unit1", unitId: 1);
            var idUnit2 = await SeedBudgetGroupAsync("Unit2", unitId: 2);

            var resultUnit1 = await CreateRepo(unitId: 1).GetByIdsAsync(new[] { idUnit1, idUnit2 });

            resultUnit1.Should().HaveCount(1);
            resultUnit1[0].Name.Should().Be("Unit1");
        }
    }
}
