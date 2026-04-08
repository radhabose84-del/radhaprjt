using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;

namespace PurchaseManagement.UnitTests.Application.Quotation.QuotationCompare.Queries
{
    public sealed class GetQuoteComparisonPendingQueryHandlerTests
    {
        private readonly Mock<IQuotationCompareQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetQuoteComparisonPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockItemLookup.Object, _mockUomLookup.Object,
                _mockHsnLookup.Object, _mockMediator.Object, _mockWorkflowLookup.Object,
                _mockUserLookup.Object, _mockIpService.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetQuoteComparisonPendingQuery
            {
                PageNumber = 1,
                PageSize = 15,
                SearchTerm = "compare"
            };
            query.PageNumber.Should().Be(1);
            query.SearchTerm.Should().Be("compare");
        }
    }
}
