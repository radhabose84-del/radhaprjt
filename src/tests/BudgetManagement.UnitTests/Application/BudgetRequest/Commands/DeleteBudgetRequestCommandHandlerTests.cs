using BudgetManagement.Application.BudgetRequest.Commands.Delete;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Commands
{
    public sealed class DeleteBudgetRequestCommandHandlerTests
    {
        private readonly Mock<IBudgetRequestCommandRepository> _mockBudgetRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteBudgetRequestCommandHandler CreateSut() =>
            new(_mockBudgetRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingEntity_CallsSoftDelete()
        {
            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetRequestBuilders.ValidEntity(1));

            _mockBudgetRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteBudgetRequestCommand { Id = 1 }, CancellationToken.None);

            _mockBudgetRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingEntity_PublishesAuditEvent()
        {
            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BudgetRequestBuilders.ValidEntity(1));

            _mockBudgetRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeleteBudgetRequestCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetRequest?)null);

            await CreateSut().Handle(new DeleteBudgetRequestCommand { Id = 99 }, CancellationToken.None);

            _mockBudgetRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotPublishAuditEvent()
        {
            _mockBudgetRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.BudgetRequest?)null);

            await CreateSut().Handle(new DeleteBudgetRequestCommand { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
