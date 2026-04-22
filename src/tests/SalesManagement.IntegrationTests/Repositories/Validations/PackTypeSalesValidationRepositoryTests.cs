using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="PackTypeSalesValidationRepository"/>.
    /// Verifies EXISTS queries against DispatchAdviceDetail, InvoiceDetail,
    /// and SalesOrderDetail for PackTypeId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PackTypeSalesValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PackTypeSalesValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeSalesValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.DispatchAdviceDetail",
                "Sales.DispatchAdviceHeader",
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader",
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader");
        }

        // -----------------------------------------------------------------------
        // HasLinkedPackTypeAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedPackTypeAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedPackTypeAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPackTypeAsync_Should_Return_True_When_SalesOrderDetail_References_PackTypeId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, packTypeId: 77);

            var result = await CreateRepo().HasLinkedPackTypeAsync(77);

            result.Should().BeTrue();
        }

        // -----------------------------------------------------------------------
        // HasActivePackTypeAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActivePackTypeAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActivePackTypeAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActivePackTypeAsync_Should_Return_True_When_Link_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, packTypeId: 88);

            var result = await CreateRepo().HasActivePackTypeAsync(88);

            result.Should().BeTrue();
        }
    }
}
