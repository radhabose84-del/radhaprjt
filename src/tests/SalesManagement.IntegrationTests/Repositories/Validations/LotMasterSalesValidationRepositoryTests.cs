using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="LotMasterSalesValidationRepository"/>.
    /// Verifies EXISTS queries against DeliveryChallanDetail, DispatchAdviceDetail,
    /// InvoiceDetail, and StoReceiptDetail for LotId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class LotMasterSalesValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LotMasterSalesValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private LotMasterSalesValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.DeliveryChallanDetail",
                "Sales.DeliveryChallanHeader",
                "Sales.DispatchAdviceDetail",
                "Sales.DispatchAdviceHeader",
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader",
                "Sales.StoReceiptDetail",
                "Sales.StoReceiptHeader");
        }

        // -----------------------------------------------------------------------
        // HasLinkedLotMasterAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedLotMasterAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedLotMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedLotMasterAsync_Should_Return_True_When_DeliveryChallanDetail_References_LotId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedDeliveryChallanHeaderAsync(conn);
            await ValidationTestHelper.SeedDeliveryChallanDetailAsync(conn, headerId, lotId: 42);

            var result = await CreateRepo().HasLinkedLotMasterAsync(42);

            result.Should().BeTrue();
        }

        // -----------------------------------------------------------------------
        // HasActiveLotMasterAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActiveLotMasterAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActiveLotMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveLotMasterAsync_Should_Return_True_When_Link_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var headerId = await ValidationTestHelper.SeedDeliveryChallanHeaderAsync(conn);
            await ValidationTestHelper.SeedDeliveryChallanDetailAsync(conn, headerId, lotId: 55);

            var result = await CreateRepo().HasActiveLotMasterAsync(55);

            result.Should().BeTrue();
        }
    }
}
