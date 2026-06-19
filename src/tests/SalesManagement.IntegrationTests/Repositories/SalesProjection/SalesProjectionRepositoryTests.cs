using Microsoft.Data.SqlClient;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;
using SalesManagement.Infrastructure.Repositories.Reports.SalesProjection;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.SalesProjection
{
    [Collection("DatabaseCollection")]
    public sealed class SalesProjectionRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesProjectionRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesProjectionRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        // The report SQL spans Sales.SalesLead/SalesQuotationHeader/SalesOrderHeader/InvoiceHeader/MiscMaster —
        // all in the Sales schema created by EnsureCreated. This proves the multi-table query executes.

        [Fact]
        public async Task GetProjectionAsync_Should_Return_NonNull_Report()
        {
            var result = await CreateRepo().GetProjectionAsync(
                ProjectionPeriodType.Monthly,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Periods.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProjectionAsync_EmptyData_Should_Return_Zero_Summary()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetProjectionAsync(
                ProjectionPeriodType.Monthly,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Summary.Should().NotBeNull();
            result.Summary!.TotalLeads.Should().Be(0);
            result.Summary.TotalOrders.Should().Be(0);
        }

        [Fact]
        public async Task GetProjectionAsync_Yearly_Should_Run()
        {
            var result = await CreateRepo().GetProjectionAsync(
                ProjectionPeriodType.Yearly,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
