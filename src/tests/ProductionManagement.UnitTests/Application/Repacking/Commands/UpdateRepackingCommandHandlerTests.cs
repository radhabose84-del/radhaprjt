using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster;

namespace ProductionManagement.UnitTests.Application.Repacking.Commands
{
    public sealed class UpdateRepackingMasterCommandHandlerTests
    {
        private readonly Mock<IRepackingMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRepackingMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateRepackingMasterCommand BuildValidCommand(int id = 1) => new()
        {
            Id = id,
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            OldPackTypeId = 1,
            OldNetWeightPerPack = 50m,
            OldStartPackNo = 1,
            OldEndPackNo = 10,
            OldTotalBags = 10,
            OldNetWeight = 500m,
            OldWarehouseId = 1,
            OldBinId = 1,
            PackTypeId = 2,
            NetWeightPerPack = 25m,
            TotalBags = 20,
            NetWeight = 500m,
            WarehouseId = 2,
            BinId = 2,
            LooseConeKgs = 0m,
            Remarks = "Updated repack",
            IsActive = 1
        };

        private void SetupHappyPath(int returnId = 1)
        {
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(It.IsAny<UpdateRepackingMasterCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingMaster { Id = 1 });

            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingMaster>()))
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
            result.Message.Should().Be("Repacking updated successfully.");
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingMaster>()), Times.Once);
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
                        e.ActionCode == "REPACKING_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity()
        {
            SetupHappyPath();
            var command = BuildValidCommand(5);
            await CreateSut().Handle(command, CancellationToken.None);
            _mockMapper.Verify(m => m.Map<ProductionManagement.Domain.Entities.RepackingMaster>(command), Times.Once);
        }
    }
}
