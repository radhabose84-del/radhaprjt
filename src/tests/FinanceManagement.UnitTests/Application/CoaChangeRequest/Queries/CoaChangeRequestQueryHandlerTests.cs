using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.CoaChangeRequest.Dto;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaChangeRequests;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaUnfreezeRequestById;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetPostFreezeChangeLog;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;

namespace FinanceManagement.UnitTests.Application.CoaChangeRequest.Queries
{
    public sealed class CoaChangeRequestQueryHandlerTests
    {
        private readonly Mock<ICoaChangeRequestQueryRepository> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        public CoaChangeRequestQueryHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        [Fact]
        public async Task GetChangeRequests_ReturnsPagedDataWithTotal()
        {
            _mockQuery.Setup(r => r.GetChangeRequestsAsync(1, "ImpactApproved", 2, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<CoaChangeRequestDto> { new() { Id = 1 } }, 11));

            var sut = new GetCoaChangeRequestsQueryHandler(_mockQuery.Object, _mockIp.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetCoaChangeRequestsQuery { Status = "ImpactApproved", PageNumber = 2, PageSize = 5 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
        }

        [Fact]
        public async Task GetUnfreezeById_ReturnsDto()
        {
            _mockQuery.Setup(r => r.GetUnfreezeRequestByIdAsync(50, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CoaUnfreezeRequestDto { Id = 50, Status = "WindowOpen" });

            var sut = new GetCoaUnfreezeRequestByIdQueryHandler(_mockQuery.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetCoaUnfreezeRequestByIdQuery { Id = 50 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(50);
        }

        // AC3 — both approver names are enriched via IUserLookup.
        [Fact]
        public async Task GetPostFreezeChangeLog_EnrichesBothApproverNames()
        {
            _mockQuery.Setup(r => r.GetPostFreezeChangeLogAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PostFreezeChangeLogDto>
                {
                    new() { ChangeRequestId = 1, CfoApproverUserId = 7, SysAdminApproverUserId = 9, IsPostFreeze = true }
                });

            var mockUserLookup = new Mock<IUserLookup>(MockBehavior.Loose);
            mockUserLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto>
                {
                    new() { UserId = 7, FirstName = "Carol", LastName = "Cfo" },
                    new() { UserId = 9, FirstName = "Sam", LastName = "Admin" }
                });

            var sut = new GetPostFreezeChangeLogQueryHandler(_mockQuery.Object, mockUserLookup.Object, _mockIp.Object, _mockMediator.Object);
            var result = await sut.Handle(new GetPostFreezeChangeLogQuery(), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].CfoApproverName.Should().Be("Carol Cfo");
            result[0].SysAdminApproverName.Should().Be("Sam Admin");
        }
    }
}
