using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="SalesUomValidationRepository"/>.
    /// Verifies EXISTS queries against SalesOrderDetail (SaleUOMId),
    /// InvoiceDetail (UOMId), DeliveryChallanDetail (UOMId),
    /// StoDetail (UOMId), and StoReceiptDetail (UOMId).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesUomValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesUomValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesUomValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader",
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader",
                "Sales.DeliveryChallanDetail",
                "Sales.DeliveryChallanHeader",
                "Sales.StoDetail",
                "Sales.StoHeader",
                "Sales.StoReceiptDetail",
                "Sales.StoReceiptHeader");
        }

        // -----------------------------------------------------------------------
        // HasLinkedUomAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedUomAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_SalesOrderDetail_References_SaleUOMId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, saleUomId: 42);

            var result = await CreateRepo().HasLinkedUomAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_InvoiceDetail_References_UOMId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedInvoiceHeaderAsync(conn);
            await ValidationTestHelper.SeedInvoiceDetailAsync(conn, headerId, uomId: 55);

            var result = await CreateRepo().HasLinkedUomAsync(55);

            result.Should().BeTrue();
        }

        // -----------------------------------------------------------------------
        // HasActiveUomAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActiveUomAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_True_When_Link_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, saleUomId: 66);

            var result = await CreateRepo().HasActiveUomAsync(66);

            result.Should().BeTrue();
        }
    }
}
