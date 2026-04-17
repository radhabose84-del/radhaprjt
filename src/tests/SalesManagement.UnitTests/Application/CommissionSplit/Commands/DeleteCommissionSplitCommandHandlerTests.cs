using MediatR;
using SalesManagement.Domain.Events;
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Commands
{
    public sealed class DeleteCommissionSplitCommandHandlerTests
    {
        private readonly Mock<ICommissionSplitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteCommissionSplitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteCommissionSplitCommand(7), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonexistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteCommissionSplitCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonexistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            try { await CreateSut().Handle(new DeleteCommissionSplitCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteCommissionSplitCommand(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "COMMISSION_SPLIT_DELETE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
