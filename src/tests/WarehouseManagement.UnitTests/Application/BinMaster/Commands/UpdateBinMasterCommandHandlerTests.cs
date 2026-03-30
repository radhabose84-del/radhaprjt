using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Events;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Commands
{
    public sealed class UpdateBinMasterCommandHandlerTests
    {
        private readonly Mock<IBinMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IBinMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IBinCodeGenerator> _mockCodeGen = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateBinMasterCommandHandler CreateSut() =>
            new(_mockCmdRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object, _mockCodeGen.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsEntityId()
        {
            var entity = BinMasterBuilders.ValidEntity();
            var command = BinMasterBuilders.ValidUpdateCommand();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.BinMaster>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity.Id);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(entity.Id);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            var command = BinMasterBuilders.ValidUpdateCommand(id: 999);
            _mockCmdRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseManagement.Domain.Entities.BinMaster?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var entity = BinMasterBuilders.ValidEntity();
            var command = BinMasterBuilders.ValidUpdateCommand();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.BinMaster>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity.Id);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "BIN_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RackIdChanged_RegeneresCode()
        {
            var entity = BinMasterBuilders.ValidEntity();
            entity.RackId = 1;
            var command = BinMasterBuilders.ValidUpdateCommand();
            command.RackId = 2; // different

            _mockCmdRepo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.UpdateAsync(It.IsAny<WarehouseManagement.Domain.Entities.BinMaster>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity.Id);
            _mockCodeGen
                .Setup(g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("BIN-NEW-CODE");

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCodeGen.Verify(
                g => g.GenerateAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
