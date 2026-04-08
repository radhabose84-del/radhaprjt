using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderById;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.UnitTests.Application.RepackingHeader.Queries
{
    public sealed class GetRepackingHeaderByIdQueryHandlerTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingHeaderByIdQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new RepackingHeaderDto { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<RepackingHeaderDto>(dto)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetRepackingHeaderByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RepackingHeaderDto?)null);

            var result = await CreateSut().Handle(new GetRepackingHeaderByIdQuery { Id = 99 }, CancellationToken.None);
            result.Should().BeNull();
        }
    }
}
