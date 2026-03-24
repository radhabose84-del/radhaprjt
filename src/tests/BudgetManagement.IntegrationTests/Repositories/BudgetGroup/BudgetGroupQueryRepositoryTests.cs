using BudgetManagement.Infrastructure.Repositories.BudgetGroup;
using BudgetManagement.Application.BudgetGroups;
using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetGroup
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetGroupQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            return new BudgetGroupQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                ipMock.Object);
        }

        private BudgetGroupCommandRepository CreateCommandRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedEntityAsync(string name = "Test Group", int unitId = 1, int deptId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(
                new BudgetManagement.Domain.Entities.BudgetGroup
                {
                    Name = name,
                    Description = "Test",
                    UnitId = unitId,
                    DepartmentId = deptId,
                    CostCenterId = 1,
                    CurrencyId = 1,
                    CarryForward = false,
                    IsParent = true,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Budget.BudgetGroup");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var filter = new BudgetGroupListFilterDto { PageNumber = 1, PageSize = 10 };
            var (items, total) = await CreateQueryRepo().GetAllAsync(filter, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var filter = new BudgetGroupListFilterDto { PageNumber = 1, PageSize = 10 };
            var (items, total) = await CreateQueryRepo().GetAllAsync(filter, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Alpha Group");
            await SeedEntityAsync("Beta Group");

            var filter = new BudgetGroupListFilterDto { PageNumber = 1, PageSize = 10, SearchTerm = "Alpha" };
            var (items, total) = await CreateQueryRepo().GetAllAsync(filter, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Alpha Group");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Group A");
            await SeedEntityAsync("Group B");
            await SeedEntityAsync("Group C");

            var filter = new BudgetGroupListFilterDto { PageNumber = 1, PageSize = 2 };
            var (page1Items, total) = await CreateQueryRepo().GetAllAsync(filter, CancellationToken.None);

            page1Items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("My Budget Group");

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result.Name.Should().Be("My Budget Group");
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
