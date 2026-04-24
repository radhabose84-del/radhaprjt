using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class SalesMiscMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesMiscMasterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesMiscMasterLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesMiscMasterLookupRepository(conn);
        }

        private async Task SeedMiscAsync(
            string code,
            string description,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.MiscMaster NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.MiscMaster
                    (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (1, @Code, @Description, 1, @IsActive, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.MiscMaster CHECK CONSTRAINT ALL;",
                new { Code = code, Description = description, IsActive = isActive, IsDeleted = isDeleted });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllMiscMasterDependentsAsync();

        [Fact]
        public async Task GetByCodeAsync_Returns_MatchingRow()
        {
            await ClearAsync();
            await SeedMiscAsync("Pending", "Pending approval");

            var result = await CreateRepo().GetByCodeAsync("Pending");

            result.Should().NotBeNull();
            result!.Code.Should().Be("Pending");
            result.Description.Should().Be("Pending approval");
        }

        [Fact]
        public async Task GetByCodeAsync_Is_CaseInsensitive()
        {
            await ClearAsync();
            await SeedMiscAsync("Pending", "Pending approval");

            var result = await CreateRepo().GetByCodeAsync("pending");

            result.Should().NotBeNull();
            result!.Code.Should().Be("Pending");
        }

        [Fact]
        public async Task GetByCodeAsync_Returns_Null_When_NoMatch()
        {
            await ClearAsync();
            await SeedMiscAsync("Pending", "Pending approval");

            var result = await CreateRepo().GetByCodeAsync("Unknown");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByCodeAsync_Excludes_Inactive()
        {
            await ClearAsync();
            await SeedMiscAsync("Pending", "Pending approval", isActive: false);

            var result = await CreateRepo().GetByCodeAsync("Pending");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByCodeAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedMiscAsync("Pending", "Pending approval", isDeleted: true);

            var result = await CreateRepo().GetByCodeAsync("Pending");

            result.Should().BeNull();
        }
    }
}
