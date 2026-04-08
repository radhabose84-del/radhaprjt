using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;
using PurchaseManagement.Application.Purchase.DutyMaster.Update;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Commands
{
    public sealed class UpdateDutyMasterCommandHandlerTests
    {
        private readonly Mock<IDutyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDutyMasterCommandRepository> _mockWriteRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateDutyMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockWriteRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = DutyMasterBuilders.ValidEntity(id);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map(It.IsAny<object>(), It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>()))
                .Returns(entity);
            _mockWriteRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var command = DutyMasterBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NullModel_ReturnsFalse()
        {
            var command = new PurchaseManagement.Application.Purchase.DutyMaster.Command.Update.UpdateDutyMasterCommand
            {
                Model = null!
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ZeroId_ReturnsFalse()
        {
            var command = DutyMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsFalse()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.DutyMaster?)null);

            var command = DutyMasterBuilders.ValidUpdateCommand(id: 999);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(DutyMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockWriteRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(DutyMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
