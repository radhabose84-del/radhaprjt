using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Commands
{
    public sealed class CreateRackMasterCommandHandlerTests
    {
        private readonly Mock<IRackMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IRackCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private CreateRackMasterCommandHanlder CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockCodeGen.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockCodeGen
                .Setup(g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync("RK-WH1-F1-A1-L1");

            _mockMapper
                .Setup(m => m.Map<WarehouseManagement.Domain.Entities.RackMaster>(It.IsAny<CreateRackMasterCommand>()))
                .Returns(RackMasterBuilders.ValidEntity(0));

            _mockCmdRepo
                .Setup(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(42);
            var command = RackMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            var command = RackMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.CreateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            var command = RackMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RACK_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesRackCode()
        {
            SetupHappyPath();
            var command = RackMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCodeGen.Verify(
                g => g.GenerateAsync(command.WarehouseId, command.FloorId, command.AisleId, command.RackLevelId),
                Times.Once);
        }
    }
}
