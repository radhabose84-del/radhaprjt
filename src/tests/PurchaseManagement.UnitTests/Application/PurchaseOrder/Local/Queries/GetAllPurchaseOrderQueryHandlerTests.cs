using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Queries
{
    public sealed class GetAllPurchaseOrderQueryHandlerTests
    {
        private readonly Mock<IPurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IBudgetGroupLookup> _mockBudgetGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockInventoryCategoryLookup = new(MockBehavior.Loose);

        private GetPurchaseOrdersQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockPartyLookup.Object, _mockBudgetGroupLookup.Object,
                _mockInventoryCategoryLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPurchaseOrdersQuery(1, 15, "search", null, null, null);
            query.PageNumber.Should().Be(1);
            query.PageSize.Should().Be(15);
            query.SearchTerm.Should().Be("search");
        }
    }
}
