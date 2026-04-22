using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.QuotationCompare
{
    /// <summary>
    /// Integration tests for QuotationCompareQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Quotation compare queries use Dapper SQL joining:
    /// - Purchase.QuotationComparisonHeader, Purchase.QuotationComparisonDetail
    /// - Purchase.QuotationHeader, Purchase.QuotationLine
    /// - Purchase.RfqMaster, Purchase.MiscMaster (status lookups)
    ///
    /// Constructor requires: IDbConnection, IIPAddressService, IMiscMasterQueryRepository
    ///
    /// GetByIdQuoteCompareAsync is a simple single-table query (testable).
    /// GetQuoteComparisonAsync requires full RFQ + Quotation + Comparison chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class QuotationCompareQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QuotationCompareQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QuotationCompareQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new QuotationCompareQueryRepository(conn, _fixture.IpMock.Object, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdQuoteCompareAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdQuoteCompareAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
