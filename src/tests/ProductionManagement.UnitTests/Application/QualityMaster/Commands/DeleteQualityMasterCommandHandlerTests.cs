using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster;

namespace ProductionManagement.UnitTests.Application.QualityMaster.Commands
{
    public sealed class DeleteQualityMasterCommandHandlerTests
    {
        private readonly Mock<IQualityMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteQualityMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteQualityMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteQualityMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteQualityMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
