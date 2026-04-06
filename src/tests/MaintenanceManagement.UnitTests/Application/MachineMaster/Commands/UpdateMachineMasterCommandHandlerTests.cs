using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Commands
{
    public sealed class UpdateMachineMasterCommandHandlerTests
    {
        private readonly Mock<IMachineMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateMachineMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateMachineMasterCommand ValidCommand() => new()
        {
            Id = 1, MachineName = "Machine A", IsActive = 1
        };

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.IsMachineLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<UpdateMachineMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            _mockQueryRepo.Setup(r => r.IsMachineLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<UpdateMachineMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineMaster>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsMachineLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineMaster>(It.IsAny<UpdateMachineMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineMaster());

            var command = new UpdateMachineMasterCommand { Id = 1, MachineName = "Machine A", IsActive = 0 };
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
