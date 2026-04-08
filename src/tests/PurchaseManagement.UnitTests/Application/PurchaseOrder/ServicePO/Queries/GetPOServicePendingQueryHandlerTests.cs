using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetPOServicePendingQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetPOServicePendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockWorkflowLookup.Object,
                _mockUserLookup.Object, _mockPartyLookup.Object, _mockIpService.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPOServicePendingQuery
            {
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "pending",
                PoId = 5
            };
            query.PageNumber.Should().Be(2);
            query.PoId.Should().Be(5);
        }
    }
}
