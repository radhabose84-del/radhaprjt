using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Application.ProductionPack.Queries.GetProductionByPackRange;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Queries
{
    public sealed class GetProductionByPackRangeQueryHandlerTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProductionByPackRangeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidRange_ReturnsDetailList()
        {
            var details = new List<ProductionPackDetailDto>
            {
                new() { Id = 1, StartPackNo = 1, EndPackNo = 10 }
            };
            _mockQueryRepo
                .Setup(r => r.GetByPackRangeAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(details);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetProductionByPackRangeQuery { StartPackNo = 1, EndPackNo = 10 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyRange_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetByPackRangeAsync(100, 200, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductionPackDetailDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetProductionByPackRangeQuery { StartPackNo = 100, EndPackNo = 200 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByPackRangeAsync(1, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductionPackDetailDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetProductionByPackRangeQuery { StartPackNo = 1, EndPackNo = 5 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
