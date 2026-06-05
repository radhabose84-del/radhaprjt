using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.RawMaterialPO.Commands
{
    public sealed class CreateRawMaterialPOCommandHandlerTests
    {
        private readonly Mock<IRawMaterialPOCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IRawMaterialPOQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);

        private CreateRawMaterialPOCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockDocSeq.Object, _mockIp.Object, _mockMisc.Object);

        private void SetupHappyPath(int newId = 1, decimal ocrQuantity = 800m, decimal alreadyConverted = 0m)
        {
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq
                .Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq
                .Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "RMPO-2026-0001" });
            _mockMapper
                .Setup(m => m.Map<RawMaterialPOHeader>(It.IsAny<object>()))
                .Returns(RawMaterialPOBuilders.ValidEntity(newId));
            _mockQueryRepo.Setup(q => q.GetOcrQuantityAsync(It.IsAny<int>())).ReturnsAsync(ocrQuantity);
            _mockQueryRepo.Setup(q => q.GetConvertedQuantityAsync(It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(alreadyConverted);
            _mockMisc
                .Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 9 });
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesPONumber()
        {
            SetupHappyPath();
            await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockDocSeq.Verify(d => d.GenerateDocumentNumber(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsPayloadValuesVerbatim()
        {
            RawMaterialPOHeader? captured = null;
            SetupHappyPath();
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<RawMaterialPOHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<RawMaterialPOHeader, int, CancellationToken>((e, _, __) => captured = e)
                .ReturnsAsync(1);

            var command = RawMaterialPOBuilders.ValidCreateCommand(quantity: 500m, rate: 68500m);
            await CreateSut().Handle(command, CancellationToken.None);

            captured.Should().NotBeNull();
            var expected = command.Details.Single();
            var line = captured!.RawMaterialPODetails!.Single();
            line.ItemValue.Should().Be(expected.ItemValue);
            line.CGSTValue.Should().Be(expected.CGSTValue);
            line.SGSTValue.Should().Be(expected.SGSTValue);
            line.TotalGST.Should().Be(expected.TotalGST);
            line.NetValue.Should().Be(expected.NetValue);
        }

        [Fact]
        public async Task Handle_PartialConversion_SetsPartiallyConvertedStatus()
        {
            SetupHappyPath(ocrQuantity: 800m, alreadyConverted: 0m);
            await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(quantity: 500m), CancellationToken.None);

            _mockMisc.Verify(
                m => m.GetMiscMasterByName(MiscEnumEntity.ConversionStatus, MiscEnumEntity.PartiallyConverted),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FullConversion_SetsFullyConvertedStatus()
        {
            SetupHappyPath(ocrQuantity: 800m, alreadyConverted: 300m);
            await CreateSut().Handle(RawMaterialPOBuilders.ValidCreateCommand(quantity: 500m), CancellationToken.None);

            _mockMisc.Verify(
                m => m.GetMiscMasterByName(MiscEnumEntity.ConversionStatus, MiscEnumEntity.FullyConverted),
                Times.Once);
        }
    }
}
