using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Queries.GetLastEndPackNo;

namespace ProductionManagement.UnitTests.Application.ProductionPack.Queries
{
    public sealed class GetLastEndPackNoQueryHandlerTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLastEndPackNoQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLastEndPackNo()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastEndPackNoAsync(2024))
                .ReturnsAsync(500);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLastEndPackNoQuery { ProductionYear = 2024 }, CancellationToken.None);

            result.Should().Be(500);
        }

        [Fact]
        public async Task Handle_ZeroYear_ReturnsZero()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastEndPackNoAsync(0))
                .ReturnsAsync(0);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetLastEndPackNoQuery { ProductionYear = 0 }, CancellationToken.None);

            result.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastEndPackNoAsync(2024))
                .ReturnsAsync(100);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetLastEndPackNoQuery { ProductionYear = 2024 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
