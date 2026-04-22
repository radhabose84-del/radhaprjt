using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="SalesCurrencyValidationRepository"/>.
    /// Verifies EXISTS queries against ItemPriceMaster and SalesSegment
    /// for CurrencyId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesCurrencyValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesCurrencyValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesCurrencyValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            await _fixture.ClearTablesAsync(
                "Sales.ItemPriceMaster",
                "Sales.AgentCommissionSlab",
                "Sales.AgentCommissionPaymentTerm",
                "Sales.AgentCommissionSalesGroup",
                "Sales.AgentCommissionConfig",
                "Sales.SalesSegment");
        }

        // -----------------------------------------------------------------------
        // HasLinkedCurrencyAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedCurrencyAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Return_True_When_SalesSegment_References_CurrencyId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedSalesSegmentAsync(conn, currencyId: 42);

            var result = await CreateRepo().HasLinkedCurrencyAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedCurrencyAsync_Should_Ignore_SoftDeleted_Records()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Seed a soft-deleted SalesSegment
            await ValidationTestHelper.SeedSalesSegmentAsync(conn, currencyId: 55, isDeleted: 1);

            var result = await CreateRepo().HasLinkedCurrencyAsync(55);

            result.Should().BeFalse();
        }
    }
}
