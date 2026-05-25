using AutoMapper;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Create;
using PurchaseManagement.UnitTests.TestData;
using Contracts.Interfaces;

namespace PurchaseManagement.UnitTests.Application.DutyMaster.Commands
{
    public sealed class CreateDutyMasterCommandHandlerTests
    {
        private readonly Mock<IDutyMasterCommandRepository> _mockWriteRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateDutyMasterCommandHandler CreateSut() =>
            new(_mockWriteRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDocSeq.Object, _mockIp.Object);

        private void SetupHappyPath(int newId = 1, string generatedCode = "DC001")
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { generatedCode });
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.DutyMaster>(It.IsAny<object>()))
                .Returns(DutyMasterBuilders.ValidEntity(newId));
            _mockWriteRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
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
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesDocumentNumber()
        {
            SetupHappyPath();
            var command = DutyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockDocSeq.Verify(
                d => d.GenerateDocumentNumber(It.IsAny<int>()),
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
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "DC-GENERATED" });
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.DutyMaster>(It.IsAny<object>()))
                .Returns(new PurchaseManagement.Domain.Entities.DutyMaster { TariffNumber = "1234.56.78" });
            _mockWriteRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.DutyMaster>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<PurchaseManagement.Domain.Entities.DutyMaster, int, CancellationToken>((e, _, _) => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(DutyMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            capturedEntity!.DutyCode.Should().Be("DC-GENERATED");
        }
    }
}
