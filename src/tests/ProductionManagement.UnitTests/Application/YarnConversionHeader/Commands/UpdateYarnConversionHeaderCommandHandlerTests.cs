using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader;

namespace ProductionManagement.UnitTests.Application.YarnConversionHeader.Commands
{
    public sealed class UpdateYarnConversionHeaderCommandHandlerTests
    {
        private readonly Mock<IYarnConversionHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IYarnConversionHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateYarnConversionHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateYarnConversionHeaderCommand BuildValidCommand(int id = 1) => new()
        {
            Id = id,
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
            Remarks = "Updated yarn conversion",
            IsActive = 1
        };

        private void SetupHappyPath(int returnId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.YarnConversionHeader>(It.IsAny<UpdateYarnConversionHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.YarnConversionHeader { Id = 1 });

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.YarnConversionHeader>()))
                .ReturnsAsync(returnId);

            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Yarn Conversion updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.YarnConversionHeader>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(BuildValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "YARN_CONVERSION_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
