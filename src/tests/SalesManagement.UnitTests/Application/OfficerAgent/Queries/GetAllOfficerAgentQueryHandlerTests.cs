using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Queries
{
    public class GetAllOfficerAgentQueryHandlerTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllOfficerAgentQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllOfficerAgentQueryHandler(_mockQueryRepo.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<OfficerAgentGroupedDto>
            {
                new() { MarketingOfficerId = 1, OfficerName = "Officer A" },
                new() { MarketingOfficerId = 2, OfficerName = "Officer B" }
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllOfficerAgentQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<OfficerAgentGroupedDto> { new() { MarketingOfficerId = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "officer"))
                .ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetAllOfficerAgentQuery { PageNumber = 2, PageSize = 5, SearchTerm = "officer" },
                CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<OfficerAgentGroupedDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllOfficerAgentQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<OfficerAgentGroupedDto>(), 0));

            await CreateSut().Handle(
                new GetAllOfficerAgentQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }
    }
}
