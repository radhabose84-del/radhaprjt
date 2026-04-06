using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.UpdateMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Commands
{
    public sealed class UpdateMachineGroupUserCommandHandlerTests
    {
        private readonly Mock<IMachineGroupUserCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMachineGroupUserCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateMachineGroupUserCommand ValidCommand() => new()
        {
            Id = 1, MachineGroupId = 1, UserId = 10
        };

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<UpdateMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
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
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<UpdateMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
