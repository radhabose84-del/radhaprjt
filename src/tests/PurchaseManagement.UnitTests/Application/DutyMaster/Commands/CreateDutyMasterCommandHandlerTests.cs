using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Create;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Commands
{
    public sealed class CreateDutyMasterCommandHandlerTests
    {
        private readonly Mock<IDutyMasterCommandRepository> _mockWriteRepo = new(MockBehavior.Loose);
        private readonly Mock<IDutyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateDutyMasterCommandHandler CreateSut() =>
            new(_mockWriteRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GenerateDutyCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("DC001");
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.DutyMaster>(It.IsAny<object>()))
                .Returns(DutyMasterBuilders.ValidEntity(newId));
            _mockWriteRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var command = DutyMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = DutyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockWriteRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesDutyCode()
        {
            SetupHappyPath();
            var command = DutyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GenerateDutyCodeAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = DutyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsDutyCodeOnEntity()
        {
            PurchaseManagement.Domain.Entities.DutyMaster? capturedEntity = null;
            _mockQueryRepo
                .Setup(r => r.GenerateDutyCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("DC-GENERATED");
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.DutyMaster>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.DutyMaster { TariffNumber = "1234.56.78" });
            _mockWriteRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<CancellationToken>()))
                .Callback<PurchaseManagement.Domain.Entities.DutyMaster, CancellationToken>((e, _) => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(DutyMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            capturedEntity!.DutyCode.Should().Be("DC-GENERATED");
        }
    }
}
