using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.DeleteRepacking;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class DeleteRepackingCommandHandlerTests
    {
        private readonly Mock<IRepackingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRepackingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteRepackingCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsException()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = () => CreateSut().Handle(new DeleteRepackingCommand(999), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteRepackingCommand(1), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
