using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class GetPendingIndentQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetPendingIndentQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockUnitLookup.Object, _mockWorkflowLookup.Object, _mockDeptLookup.Object,
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
            var query = new GetPendingIndentQuery
            {
                PageNumber = 2,
                PageSize = 10,
                SearchTerm = "indent"
            };
            query.PageNumber.Should().Be(2);
            query.SearchTerm.Should().Be("indent");
        }
    }
}
