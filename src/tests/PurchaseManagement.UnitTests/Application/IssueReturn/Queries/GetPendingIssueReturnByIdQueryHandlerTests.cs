using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById;

namespace PurchaseManagement.UnitTests.Application.IssueReturn.Queries
{
    public sealed class GetPendingIssueReturnByIdQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemPurchaseToleranceLookup> _mockToleranceLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IPutawayRuleLookup> _mockPutawayLookup = new(MockBehavior.Loose);
        private readonly Mock<IRackLookup> _mockRackLookup = new(MockBehavior.Loose);
        private readonly Mock<IBinLookup> _mockBinLookup = new(MockBehavior.Loose);

        private GetPendingIssueReturnByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object,
                _mockUomLookup.Object, _mockDeptLookup.Object, _mockUnitLookup.Object,
                _mockToleranceLookup.Object, _mockWarehouseLookup.Object, _mockPutawayLookup.Object,
                _mockBinLookup.Object, _mockRackLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPendingIssueReturnByIdQuery { Id = 42 };
            query.Id.Should().Be(42);
        }
    }
}
