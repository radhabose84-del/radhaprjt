using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Queries.GetMrsPending;

namespace PurchaseManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetMrsPendingQueryHandlerTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetMrsPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockUnitLookup.Object, _mockWorkflowLookup.Object, _mockDeptLookup.Object,
                _mockUserLookup.Object, _mockIpService.Object, _mockUomLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetMrsPendingQuery
            {
                PageNumber = 1,
                PageSize = 15,
                SearchTerm = "test"
            };
            query.PageNumber.Should().Be(1);
            query.SearchTerm.Should().Be("test");
        }
    }
}
