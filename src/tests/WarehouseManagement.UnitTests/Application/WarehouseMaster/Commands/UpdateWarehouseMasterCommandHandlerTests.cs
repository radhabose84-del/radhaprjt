using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Commands
{
    public sealed class UpdateWarehouseMasterCommandHandlerTests
    {
        private readonly Mock<IWarehouseMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IWarehouseCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILocationLookup> _mockLocationLookup = new(MockBehavior.Loose);

        private UpdateWarehouseMasterCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockQueryRepo.Object, _mockCodeGen.Object,
                _mockMapper.Object, _mockMediator.Object, _mockLocationLookup.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            var entity = WarehouseMasterBuilders.ValidEntity();

            _mockCmdRepo
                .Setup(r => r.GetByIdAsync(command.Id))
                .ReturnsAsync(entity);
            _mockCmdRepo
                .Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand(id: 999);

            _mockCmdRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((WarehouseManagement.Domain.Entities.WarehouseMaster?)null);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            var entity = WarehouseMasterBuilders.ValidEntity();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>())).ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            var entity = WarehouseMasterBuilders.ValidEntity();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>())).ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.WarehouseMaster>()), Times.Once);
        }
    }
}
