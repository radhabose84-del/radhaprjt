using AutoMapper;
using MediatR;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Commands
{
    public sealed class CreateBinMasterCommandHandlerTests
    {
        private readonly Mock<IBinMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IBinCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateBinMasterCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockCodeGen.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockCodeGen
                .Setup(g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("BIN-WH1-RK1-001");

            _mockMapper
                .Setup(m => m.Map<WarehouseManagement.Domain.Entities.BinMaster>(It.IsAny<CreateBinMasterCommand>()))
                .Returns(BinMasterBuilders.ValidEntity(0));

            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.BinMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var command = BinMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = BinMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.BinMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = BinMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BIN_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesBinCode()
        {
            SetupHappyPath();
            var command = BinMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCodeGen.Verify(
                g => g.GenerateAsync(command.WarehouseId, command.RackId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
