using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ServicePO.Queries
{
    public sealed class GetSESListQueryHandlerTests
    {
        private readonly Mock<IServicePurchaseOrderQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetSESListQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockPartyLookup.Object,
                _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetSESListQuery
            {
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "ses"
            };
            query.PageNumber.Should().Be(2);
            query.SearchTerm.Should().Be("ses");
        }
    }
}
