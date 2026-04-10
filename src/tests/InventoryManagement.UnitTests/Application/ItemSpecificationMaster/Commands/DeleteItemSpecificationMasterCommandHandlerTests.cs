using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationMaster.Commands
{
    public sealed class DeleteItemSpecificationMasterCommandHandlerTests
    {
        private readonly Mock<IItemSpecificationMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteItemSpecificationMasterCommandHandler CreateSut() =>
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
                ItemSpecificationMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(1);

            await CreateSut().Handle(
                ItemSpecificationMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "ITEMSPECIFICATIONMASTER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
