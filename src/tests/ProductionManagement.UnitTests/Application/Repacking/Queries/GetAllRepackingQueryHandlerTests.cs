using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Queries.GetAllRepacking;
using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetAllRepackingQueryHandlerTests
    {
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRepackingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<RepackingHeaderDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<RepackingHeaderDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllRepackingQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var dtoList = new List<RepackingHeaderDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 0));
            _mockMapper.Setup(m => m.Map<List<RepackingHeaderDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllRepackingQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
