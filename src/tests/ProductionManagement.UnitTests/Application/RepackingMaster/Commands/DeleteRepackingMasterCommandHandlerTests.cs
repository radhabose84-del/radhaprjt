using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader;

namespace ProductionManagement.UnitTests.Application.RepackingMaster.Commands
{
    public sealed class DeleteRepackingMasterCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteRepackingHeaderCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteRepackingHeaderCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteRepackingHeaderCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "REPACKING_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteRepackingHeaderCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            try { await CreateSut().Handle(new DeleteRepackingHeaderCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
