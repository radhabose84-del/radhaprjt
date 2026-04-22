using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Validations;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for <see cref="PartyMasterSalesValidationRepository"/>.
    /// Verifies EXISTS queries against SalesOrderHeader (PartyId, AgentId, SubAgentId),
    /// InvoiceHeader, SalesQuotationHeader, SalesReturnHeader, DispatchAdviceHeader,
    /// SalesEnquiryHeader, ComplaintHeader, CustomerVisit, AgentCommissionConfig,
    /// AgentCustomerMapping, DispatchAddressMapping, SalesContact, SalesLead, OfficerAgent.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterSalesValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterSalesValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PartyMasterSalesValidationRepository CreateRepo()
            => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearDependentTablesAsync()
        {
            // Clear in FK-safe order (children first)
            await _fixture.ClearTablesAsync(
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader",
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader",
                "Sales.SalesQuotationDetail",
                "Sales.SalesQuotationHeader",
                "Sales.SalesReturnDetail",
                "Sales.SalesReturnHeader",
                "Sales.DispatchAdviceDetail",
                "Sales.DispatchAdviceHeader",
                "Sales.SalesEnquiryDetail",
                "Sales.SalesEnquiryHeader",
                "Sales.ComplaintResolution",
                "Sales.ComplaintDetail",
                "Sales.ComplaintHeader",
                "Sales.CustomerVisitProduct",
                "Sales.CustomerVisit",
                "Sales.AgentCommissionSlab",
                "Sales.AgentCommissionPaymentTerm",
                "Sales.AgentCommissionSalesGroup",
                "Sales.AgentCommissionConfig",
                "Sales.AgentCustomerMapping",
                "Sales.DispatchAddressMapping",
                "Sales.SalesContact",
                "Sales.SalesLead",
                "Sales.OfficerAgent");
        }

        // -----------------------------------------------------------------------
        // HasLinkedPartyMasterAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasLinkedPartyMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_True_When_SalesOrderHeader_References_PartyId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn, partyId: 42);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Return_True_When_SalesContact_References_PartyId()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedSalesContactAsync(conn, partyId: 55);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(55);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Should_Ignore_SoftDeleted_Headers()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Soft-deleted SalesOrderHeader should NOT count as a link
            await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn, partyId: 66, isDeleted: 1);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(66);

            result.Should().BeFalse();
        }

        // -----------------------------------------------------------------------
        // HasActivePartyMasterAsync
        // -----------------------------------------------------------------------

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_False_When_No_Links()
        {
            await ClearDependentTablesAsync();

            var result = await CreateRepo().HasActivePartyMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Return_True_When_Active_SalesOrderHeader_Exists()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn, partyId: 77);

            var result = await CreateRepo().HasActivePartyMasterAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Should_Ignore_Inactive_Headers()
        {
            await ClearDependentTablesAsync();

            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Inactive (IsActive=0) header should NOT count
            await ValidationTestHelper.SeedSalesOrderHeaderAsync(conn, partyId: 88, isActive: 0);

            var result = await CreateRepo().HasActivePartyMasterAsync(88);

            result.Should().BeFalse();
        }
    }
}
