using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader;

namespace PurchaseManagement.UnitTests.Application.Issue.Queries
{
    public sealed class GetPendingIssueHeaderQueryHandlerTests
    {
        private readonly Mock<IIssueQueryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);

        private GetPendingIssueHeaderQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockUnitLookup.Object, _mockDeptLookup.Object, _mockWarehouseLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetPendingIssueHeaderQuery
            {
                FromDate = DateTimeOffset.UtcNow.AddDays(-7),
                ToDate = DateTimeOffset.UtcNow,
                PageNumber = 2,
                PageSize = 20,
                SearchTerm = "test"
            };
            query.PageNumber.Should().Be(2);
            query.SearchTerm.Should().Be("test");
        }
    }
}
