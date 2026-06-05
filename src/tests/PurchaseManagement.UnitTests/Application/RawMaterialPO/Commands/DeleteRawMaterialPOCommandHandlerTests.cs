using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Commands
{
    public sealed class DeleteRawMaterialPOCommandHandlerTests
    {
        private readonly Mock<IRawMaterialPOCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRawMaterialPOCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteRawMaterialPOCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteRawMaterialPOCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new DeleteRawMaterialPOCommand(99), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();
            try { await sut.Handle(new DeleteRawMaterialPOCommand(99), CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
