using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.SalesQuotationAmendment;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.SalesQuotationAmendment
{
    [Collection("DatabaseCollection")]
    public sealed class SalesQuotationAmendmentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesQuotationAmendmentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // Item/HSN lookups mocked Loose; the methods tested below are SQL-only and never invoke them.
        private SalesQuotationAmendmentQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString),
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<IHSNLookup>(MockBehavior.Loose).Object);

        [Fact]
        public async Task GetAllAsync_EmptyData_Should_Return_Empty()
        {
            await _fixture.ClearAllTablesAsync();

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task HasPendingAmendmentAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().HasPendingAmendmentAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesQuotationExistsAndApprovedAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().SalesQuotationExistsAndApprovedAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesQuotationDetailExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().SalesQuotationDetailExistsAsync(9999999, 9999999);
            result.Should().BeFalse();
        }
    }
}
