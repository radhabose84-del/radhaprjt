using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetImportPOPending;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Queries
{
    public sealed class GetImportPOsPendingQueryHandlerTests
    {
        private readonly Mock<IImportPOQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetImportPOsPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockItemLookup.Object, _mockMediator.Object,
                _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockPartyLookup.Object,
                _mockIpService.Object, _mockDeptLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetImportPOsPendingQuery
            {
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "import",
                PoId = 5
            };
            query.PageNumber.Should().Be(2);
            query.PoId.Should().Be(5);
        }
    }
}
