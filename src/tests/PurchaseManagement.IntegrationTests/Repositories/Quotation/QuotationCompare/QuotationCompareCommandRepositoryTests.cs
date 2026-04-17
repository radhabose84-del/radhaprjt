using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationCompare;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.QuotationCompare
{
    /// <summary>
    /// Integration tests for QuotationCompareCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Quotation Comparison aggregates quotations from multiple suppliers for an RFQ:
    /// - QuotationComparisonHeader links to RfqId, contains StatusId (approval flow)
    /// - QuotationComparisonDetail has per-supplier pricing comparisons
    /// - StatusId is fetched from MiscMaster (ApprovalStatus -> Pending) during creation
    /// - Creates/updates PriceMaster entries when comparison is approved
    ///
    /// Constructor requires: ApplicationDbContext, IIPAddressService, IMiscMasterQueryRepository
    ///
    /// Full testing requires: RfqMaster, QuotationHeader (multiple), MiscMaster chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class QuotationCompareCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QuotationCompareCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QuotationCompareCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            return new QuotationCompareCommandRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);
            repo.Should().NotBeNull();
        }

        // Note: AddAsync requires a fully populated QuotationComparisonHeader entity
        // with valid RfqId FK and MiscMaster chain for status lookup.
        // Complex approval and PriceMaster side effects are covered by unit tests.
    }
}
