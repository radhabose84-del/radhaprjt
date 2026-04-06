using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUsers.Command.CreateMachineGroupUser;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Commands
{
    public sealed class CreateMachineGroupUserCommandHandlerTests
    {
        private readonly Mock<IMachineGroupUserCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateMachineGroupUsersCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateMachineGroupUserCommand ValidCommand() => new()
        {
            MachineGroupId = 1,
            UserId = 1
        };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<CreateMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(newId: 7);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsyncOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "MachineGroupUsers Master"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<CreateMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
                .ReturnsAsync(0);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*MachineGroup Users Creation Failed*");
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_DoesNotPublishAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroupUser>(It.IsAny<CreateMachineGroupUserCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MachineGroupUser());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroupUser>()))
                .ReturnsAsync(0);

            try { await CreateSut().Handle(ValidCommand(), CancellationToken.None); } catch { }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
