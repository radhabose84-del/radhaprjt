using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Commands
{
    public sealed class UpdateRackMasterCommandHandlerTests
    {
        private readonly Mock<IRackMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IRackCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateRackMasterCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockCodeGen.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsEntityId()
        {
            var entity = RackMasterBuilders.ValidEntity();
            var command = RackMasterBuilders.ValidUpdateCommand();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>())).ReturnsAsync(entity.Id);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(entity.Id);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = RackMasterBuilders.ValidUpdateCommand(id: 999);
            _mockCmdRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WarehouseManagement.Domain.Entities.RackMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var entity = RackMasterBuilders.ValidEntity();
            var command = RackMasterBuilders.ValidUpdateCommand();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>())).ReturnsAsync(entity.Id);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RACK_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var entity = RackMasterBuilders.ValidEntity();
            var command = RackMasterBuilders.ValidUpdateCommand();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>())).ReturnsAsync(entity.Id);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmdRepo.Verify(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.RackMaster>()), Times.Once);
        }
    }
}
