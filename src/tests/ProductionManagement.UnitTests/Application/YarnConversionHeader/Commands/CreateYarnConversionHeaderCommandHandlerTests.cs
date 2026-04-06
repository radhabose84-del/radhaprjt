using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader;
using ProductionManagement.Domain.Common;

namespace ProductionManagement.UnitTests.Application.YarnConversionHeader.Commands
{
    public sealed class CreateYarnConversionHeaderCommandHandlerTests
    {
        private readonly Mock<IYarnConversionHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateYarnConversionHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);

            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.YarnConversionHeader>(It.IsAny<CreateYarnConversionHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.YarnConversionHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeYarnConversion, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync(7);

            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(7))
                .ReturnsAsync(new List<string> { "YC-001" });

            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.YarnConversionHeader>(), 7))
                .ReturnsAsync(newId);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CreateYarnConversionHeaderCommand BuildValidCommand() => new()
        {
            ConversionDate = DateOnly.FromDateTime(DateTime.Today),
            LotId = 1,
            OldItemId = 1,
            OldPackTypeId = 1,
            OldStartPackNo = 1,
            OldEndPackNo = 10,
            OldTotalBags = 10,
            OldNetWeightPerPack = 25m,
            OldNetWeight = 250m,
            OldWarehouseId = 1,
            OldBinId = 1,
            ItemId = 2,
            PackTypeId = 2,
            TotalBags = 5,
            NetWeightPerPack = 50m,
            NetWeight = 250m,
            LooseQty = 0m,
            WarehouseId = 2,
            BinId = 2,
            WasteQty = 0m,
            Remarks = "Test yarn conversion"
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<ProductionManagement.Domain.Entities.YarnConversionHeader>(), 7), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "YARN_CONVERSION_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoTransactionTypeId_ThrowsException()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.YarnConversionHeader>(It.IsAny<CreateYarnConversionHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.YarnConversionHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeYarnConversion, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync((int?)null);

            Func<Task> act = () => CreateSut().Handle(BuildValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Transaction Type*not found*");
        }

        [Fact]
        public async Task Handle_EmptyDocumentSequence_ThrowsException()
        {
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.YarnConversionHeader>(It.IsAny<CreateYarnConversionHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.YarnConversionHeader());

            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(
                    MiscEnumEntity.TransactionTypeYarnConversion, MiscEnumEntity.ModuleSales, 1))
                .ReturnsAsync(7);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(7))
                .ReturnsAsync(new List<string>());

            Func<Task> act = () => CreateSut().Handle(BuildValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No document sequence configured*");
        }
    }
}
