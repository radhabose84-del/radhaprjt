using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="SalesItemValidationRepository"/>.
    /// Verifies EXISTS queries against SalesOrderDetail, SalesQuotationDetail,
    /// InvoiceDetail, DeliveryChallanDetail, SalesEnquiryDetail, DispatchAdviceDetail,
    /// SalesReturnDetail, ComplaintDetail, ItemPriceMaster, StoDetail, StoReceiptDetail,
    /// and StockLedger for ItemId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesItemValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesItemValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesItemValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader",
                "Sales.SalesQuotationDetail",
                "Sales.SalesQuotationHeader",
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader",
                "Sales.DeliveryChallanDetail",
                "Sales.DeliveryChallanHeader",
                "Sales.SalesEnquiryDetail",
                "Sales.SalesEnquiryHeader",
                "Sales.DispatchAdviceDetail",
                "Sales.DispatchAdviceHeader",
                "Sales.SalesReturnDetail",
                "Sales.SalesReturnHeader",
                "Sales.ComplaintResolution",
                "Sales.ComplaintDetail",
                "Sales.ComplaintHeader",
                "Sales.ItemPriceMaster",
                "Sales.StoDetail",
                "Sales.StoHeader",
                "Sales.StoReceiptDetail",
                "Sales.StoReceiptHeader",
                "Sales.StockLedger");
        }

        // -----------------------------------------------------------------------
        // HasLinkedItemAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedItemAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_True_When_SalesOrderDetail_References_ItemId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn);
            await ValidationTestHelper.SeedSalesOrderDetailAsync(conn, headerId, itemId: 42);

            var result = await CreateRepo().HasLinkedItemAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_True_When_StockLedger_References_ItemId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"
                INSERT INTO Sales.StockLedger
                    (UnitId, DocType, DocNo, DetailDocNo, DocDate, ItemId, LotId,
                     PackNo, PackTypeId, WarehouseId, BinId, TotalQty, TotalValue, StatusId)
                VALUES (1, 'PROD', 1, 0, GETDATE(), @ItemId, 1, 1, 1, 1, 1, 100, 500, 1)",
                new { ItemId = 55 });

            var result = await CreateRepo().HasLinkedItemAsync(55);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_True_When_ItemPriceMaster_References_ItemId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedItemPriceMasterAsync(conn, itemId: 66);

            var result = await CreateRepo().HasLinkedItemAsync(66);

            result.Should().BeTrue();
        }

        // -----------------------------------------------------------------------
        // HasActiveItemAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActiveItemAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_True_When_Active_ItemPriceMaster_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedItemPriceMasterAsync(conn, itemId: 77);

            var result = await CreateRepo().HasActiveItemAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemAsync_Should_Ignore_SoftDeleted_ItemPriceMaster()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Soft-deleted ItemPriceMaster should NOT match HasActiveItemAsync
            await ValidationTestHelper.SeedItemPriceMasterAsync(conn, itemId: 88, isDeleted: 1);

            // Only checking ItemPriceMaster path here (no detail table rows)
            var result = await CreateRepo().HasActiveItemAsync(88);

            result.Should().BeFalse();
        }
    }
}
