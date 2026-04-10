using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Commands
{
    public sealed class DeleteItemSpecificationValueCommandHandlerTests
    {
        private readonly Mock<IItemSpecificationValueCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteItemSpecificationValueCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(1);

            var result = await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                ItemSpecificationValueBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "ITEMSPECIFICATIONVALUE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
