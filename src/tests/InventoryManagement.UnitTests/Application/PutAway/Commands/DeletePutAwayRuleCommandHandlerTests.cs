using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PutAway.Commands
{
    public sealed class DeletePutAwayRuleCommandHandlerTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeletePutAwayRuleCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsId()
        {
            var entity = new PutAwayRule { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.SoftDeleteAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeletePutAwayRuleCommand { Id = 1 }, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PutAwayRule?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new DeletePutAwayRuleCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = new PutAwayRule { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.SoftDeleteAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new DeletePutAwayRuleCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
