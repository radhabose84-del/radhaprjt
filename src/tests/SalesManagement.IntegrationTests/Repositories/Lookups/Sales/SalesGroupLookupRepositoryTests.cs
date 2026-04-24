using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class SalesGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesGroupLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesGroupLookupRepository(conn);
        }

        private async Task SeedAsync(
            string salesGroupName,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.SalesGroup NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesGroup
                    (SalesGroupName, SalesOfficeId, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@SalesGroupName, 1, @IsActive, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesGroup CHECK CONSTRAINT ALL;",
                new { SalesGroupName = salesGroupName, IsActive = isActive, IsDeleted = isDeleted });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesIncludingPrerequisitesAsync();

        [Fact]
        public async Task GetAllSalesGroupAsync_Returns_Active_NonDeleted()
        {
            await ClearAsync();
            await SeedAsync("North Group");
            await SeedAsync("South Group");

            var result = await CreateRepo().GetAllSalesGroupAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.SalesGroupName).Should().BeEquivalentTo(new[] { "North Group", "South Group" });
        }

        [Fact]
        public async Task GetAllSalesGroupAsync_Orders_By_Name_Asc()
        {
            await ClearAsync();
            await SeedAsync("Zulu");
            await SeedAsync("Alpha");
            await SeedAsync("Mike");

            var result = await CreateRepo().GetAllSalesGroupAsync();

            result.Select(r => r.SalesGroupName).Should().ContainInOrder("Alpha", "Mike", "Zulu");
        }

        [Fact]
        public async Task GetAllSalesGroupAsync_Excludes_Inactive()
        {
            await ClearAsync();
            await SeedAsync("Active Group");
            await SeedAsync("Inactive Group", isActive: false);

            var result = await CreateRepo().GetAllSalesGroupAsync();

            result.Should().ContainSingle().Which.SalesGroupName.Should().Be("Active Group");
        }

        [Fact]
        public async Task GetAllSalesGroupAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("Keep");
            await SeedAsync("Deleted", isDeleted: true);

            var result = await CreateRepo().GetAllSalesGroupAsync();

            result.Should().ContainSingle().Which.SalesGroupName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllSalesGroupAsync_Returns_Empty_When_NoData()
        {
            await ClearAsync();

            var result = await CreateRepo().GetAllSalesGroupAsync();

            result.Should().BeEmpty();
        }
    }
}
