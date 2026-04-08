using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.UnitTests.Application.RepackingHeader.Queries
{
    public sealed class GetAllRepackingHeaderQueryHandlerTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRepackingHeaderQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var data = new List<RepackingHeaderDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllAsync(1, 10, null, null)).ReturnsAsync((data, 1));
            _mockMapper.Setup(m => m.Map<List<RepackingHeaderDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllRepackingHeaderQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
