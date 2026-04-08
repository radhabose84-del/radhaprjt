using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn;

namespace PurchaseManagement.UnitTests.Application.IssueReturn.Queries
{
    public sealed class GetPendingIssueReturnQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetPendingIssueReturnQueryHandler CreateSut() =>
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
            var query = new GetPendingIssueReturnQuery
            {
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "search"
            };
            query.PageNumber.Should().Be(2);
            query.SearchTerm.Should().Be("search");
        }
    }
}
