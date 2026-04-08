using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class GetPendingIndentByIdQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IInventoryCategoryLookup> _mockCatLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetPendingIndentByIdQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockCatLookup.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPendingIndentByIdQuery { Id = 42 };
            query.Id.Should().Be(42);
        }
    }
}
