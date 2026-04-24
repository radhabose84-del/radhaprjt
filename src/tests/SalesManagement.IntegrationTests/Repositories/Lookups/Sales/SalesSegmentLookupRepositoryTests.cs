using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Lookups.Sales;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Lookups.Sales
{
    [Collection("DatabaseCollection")]
    public sealed class SalesSegmentLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesSegmentLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesSegmentLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesSegmentLookupRepository(conn);
        }

        private async Task SeedAsync(
            string segmentName,
            int salesOrganisationId = 1,
            int salesChannelId = 1,
            int businessUnitId = 1,
            bool isActive = true,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.SalesSegment NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.SalesSegment
                    (SalesOrganisationId, SalesChannelId, BusinessUnitId, CurrencyId,
                     SegmentName, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                VALUES
                    (@OrgId, @ChannelId, @BuId, 1,
                     @SegmentName, @IsActive, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.SalesSegment CHECK CONSTRAINT ALL;",
                new
                {
                    OrgId = salesOrganisationId,
                    ChannelId = salesChannelId,
                    BuId = businessUnitId,
                    SegmentName = segmentName,
                    IsActive = isActive,
                    IsDeleted = isDeleted
                });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesIncludingPrerequisitesAsync();

        [Fact]
        public async Task GetAllSalesSegmentAsync_Returns_Active_NonDeleted()
        {
            await ClearAsync();
            await SeedAsync("North", businessUnitId: 1);
            await SeedAsync("South", businessUnitId: 2);

            var result = await CreateRepo().GetAllSalesSegmentAsync();

            result.Should().HaveCount(2);
            result.Select(r => r.SegmentName).Should().BeEquivalentTo(new[] { "North", "South" });
        }

        [Fact]
        public async Task GetAllSalesSegmentAsync_Orders_By_Name_Asc()
        {
            await ClearAsync();
            await SeedAsync("Zulu", businessUnitId: 1);
            await SeedAsync("Alpha", businessUnitId: 2);
            await SeedAsync("Mike", businessUnitId: 3);

            var result = await CreateRepo().GetAllSalesSegmentAsync();

            result.Select(r => r.SegmentName).Should().ContainInOrder("Alpha", "Mike", "Zulu");
        }

        [Fact]
        public async Task GetAllSalesSegmentAsync_Excludes_Inactive()
        {
            await ClearAsync();
            await SeedAsync("Active Seg", businessUnitId: 1);
            await SeedAsync("Inactive Seg", businessUnitId: 2, isActive: false);

            var result = await CreateRepo().GetAllSalesSegmentAsync();

            result.Should().ContainSingle().Which.SegmentName.Should().Be("Active Seg");
        }

        [Fact]
        public async Task GetAllSalesSegmentAsync_Excludes_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("Keep", businessUnitId: 1);
            await SeedAsync("Deleted", businessUnitId: 2, isDeleted: true);

            var result = await CreateRepo().GetAllSalesSegmentAsync();

            result.Should().ContainSingle().Which.SegmentName.Should().Be("Keep");
        }

        [Fact]
        public async Task GetAllSalesSegmentAsync_Returns_Empty_When_NoData()
        {
            await ClearAsync();

            var result = await CreateRepo().GetAllSalesSegmentAsync();

            result.Should().BeEmpty();
        }
    }
}
