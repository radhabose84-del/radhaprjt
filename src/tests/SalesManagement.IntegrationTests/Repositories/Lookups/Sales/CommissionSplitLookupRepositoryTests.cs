using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class CommissionSplitLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CommissionSplitLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CommissionSplitLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CommissionSplitLookupRepository(conn);
        }

        private async Task SeedAsync(
            string splitCode, string splitName,
            Status isActive = Status.Active,
            IsDelete isDeleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.CommissionSplit.Add(new Domain.Entities.CommissionSplit
            {
                SplitCode = splitCode,
                SplitName = splitName,
                IsActive = isActive,
                IsDeleted = isDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("Sales.CommissionSplitDetail", "Sales.CommissionSplit");

        [Fact]
        public async Task GetAllCommissionSplitAsync_Returns_Active_NonDeleted()
        {
            await ClearAsync();
            await SeedAsync("CS1", "Beta Split");
            await SeedAsync("CS2", "Alpha Split");

            var result = await CreateRepo().GetAllCommissionSplitAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.SplitCode).Should().BeEquivalentTo(new[] { "CS1", "CS2" });
        }

        [Fact]
        public async Task GetAllCommissionSplitAsync_Orders_By_SplitName_Asc()
        {
            await ClearAsync();
            await SeedAsync("CS1", "Zulu Split");
            await SeedAsync("CS2", "Alpha Split");
            await SeedAsync("CS3", "Mike Split");

            var result = await CreateRepo().GetAllCommissionSplitAsync();

            result.Select(r => r.SplitName).Should().ContainInOrder("Alpha Split", "Mike Split", "Zulu Split");
        }

        [Fact]
        public async Task GetAllCommissionSplitAsync_Excludes_Inactive()
        {
            await ClearAsync();
            await SeedAsync("CS1", "Active");
            await SeedAsync("CS2", "Inactive", isActive: Status.Inactive);

            var result = await CreateRepo().GetAllCommissionSplitAsync();

            result.Should().ContainSingle().Which.SplitCode.Should().Be("CS1");
        }

        [Fact]
        public async Task GetAllCommissionSplitAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("CS1", "Keep");
            await SeedAsync("CS2", "Deleted", isDeleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllCommissionSplitAsync();

            result.Should().ContainSingle().Which.SplitCode.Should().Be("CS1");
        }

        [Fact]
        public async Task GetAllCommissionSplitAsync_Returns_Empty_When_NoData()
        {
            await ClearAsync();

            var result = await CreateRepo().GetAllCommissionSplitAsync();

            result.Should().BeEmpty();
        }
    }
}
