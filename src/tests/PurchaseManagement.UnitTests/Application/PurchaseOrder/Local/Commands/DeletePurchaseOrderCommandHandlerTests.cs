using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Commands
{
    public sealed class DeletePurchaseOrderCommandHandlerTests
    {
        private readonly Mock<IPurchaseOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);

        private DeletePurchaseOrderCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        [Fact]
        public async Task Handle_SuccessfulDelete_ReturnsTrue()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(new DeletePurchaseOrderCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFalse()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await CreateSut().Handle(new DeletePurchaseOrderCommand { Id = 999 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsSoftDeleteOnce()
        {
            _mockRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(new DeletePurchaseOrderCommand { Id = 1 }, CancellationToken.None);

            _mockRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
