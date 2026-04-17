using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="SalesHsnValidationRepository"/>.
    /// Verifies EXISTS queries against SalesOrderDetail and SalesQuotationDetail
    /// for HSNId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesHsnValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesHsnValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesHsnValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader",
                "Sales.SalesQuotationDetail",
                "Sales.SalesQuotationHeader");
        }

        // -----------------------------------------------------------------------
        // HasLinkedHsnAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedHsnAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedHsnAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedHsnAsync_Should_Return_True_When_SalesOrderDetail_References_HSNId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, hsnId: 42);

            var result = await CreateRepo().HasLinkedHsnAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedHsnAsync_Should_Return_True_When_SalesQuotationDetail_References_HSNId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesQuotationHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesQuotationDetailAsync(conn, headerId, hsnId: 55);

            var result = await CreateRepo().HasLinkedHsnAsync(55);

            result.Should().BeTrue();
        }

        // -----------------------------------------------------------------------
        // HasActiveHsnAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActiveHsnAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActiveHsnAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveHsnAsync_Should_Return_True_When_Link_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, hsnId: 66);

            var result = await CreateRepo().HasActiveHsnAsync(66);

            result.Should().BeTrue();
        }
    }
}
