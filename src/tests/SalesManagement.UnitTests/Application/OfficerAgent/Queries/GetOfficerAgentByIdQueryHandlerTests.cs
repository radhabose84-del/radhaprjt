using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Dto;
using SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Queries
{
    public class GetOfficerAgentByIdQueryHandlerTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetOfficerAgentByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<OfficerAgentGroupedDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as OfficerAgentGroupedDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetOfficerAgentByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var dto = new OfficerAgentGroupedDto { MarketingOfficerId = 1, OfficerName = "Officer A" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetOfficerAgentByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var dto = new OfficerAgentGroupedDto { MarketingOfficerId = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            await CreateSut().Handle(new GetOfficerAgentByIdQuery { Id = 7 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNotSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((OfficerAgentGroupedDto?)null);

            var result = await CreateSut().Handle(
                new GetOfficerAgentByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }
    }
}
