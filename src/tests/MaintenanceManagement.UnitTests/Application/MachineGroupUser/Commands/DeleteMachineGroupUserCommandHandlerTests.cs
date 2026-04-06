using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Command.DeleteMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Commands
{
    public sealed class DeleteMachineGroupUserCommandHandlerTests
    {
        private readonly Mock<IMachineGroupUserCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMachineGroupUserCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static DeleteMachineGroupUserCommand ValidCommand() => new() { Id = 1 };

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<DeleteMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
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
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<DeleteMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo.Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
