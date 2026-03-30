using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Delete;
using PurchaseManagement.Application.Purchase.DutyMaster.Delete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Commands
{
    public sealed class DeleteDutyMasterCommandHandlerTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockReadRepo = new(MockBehavior.Strict);
        private readonly Mock<IDutyMasterCommandRepository> _mockWriteRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteDutyMasterCommandHandler CreateSut() =>
            new(_mockReadRepo.Object, _mockWriteRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = DutyMasterBuilders.ValidEntity(id);
            _mockReadRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
            _mockWriteRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidId_Completes()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeleteDutyMasterCommand(1), CancellationToken.None);

            _mockWriteRepo.Verify(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeleteDutyMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            _mockReadRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.DutyMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new DeleteDutyMasterCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallSoftDelete()
        {
            _mockReadRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.DutyMaster?)null);

            try { await CreateSut().Handle(new DeleteDutyMasterCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockWriteRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
