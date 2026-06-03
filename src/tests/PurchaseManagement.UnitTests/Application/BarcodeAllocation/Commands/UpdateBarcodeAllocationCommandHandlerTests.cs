using AutoMapper;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeAllocation.Commands
{
    public sealed class UpdateBarcodeAllocationCommandHandlerTests
    {
        private readonly Mock<IBarcodeAllocationCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateBarcodeAllocationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.BarcodeAllocation>(It.IsAny<UpdateBarcodeAllocationCommand>()))
                .Returns(BarcodeAllocationBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.BarcodeAllocation>()))
                .ReturnsAsync(id);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(BarcodeAllocationBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(BarcodeAllocationBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.BarcodeAllocation>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(BarcodeAllocationBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
