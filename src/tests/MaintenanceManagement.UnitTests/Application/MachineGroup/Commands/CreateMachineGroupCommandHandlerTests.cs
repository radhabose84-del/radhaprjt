using AutoMapper;
using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Xunit;

namespace MaintenanceManagement.UnitTests.Application.MachineGroup.Commands
{
    public sealed class CreateMachineGroupCommandHandlerTests
    {
        private readonly Mock<IMachineGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMachineGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = MachineGroupBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MachineGroup>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroup>()))
                .ReturnsAsync(entity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            SetupHappyPath(5);
            var result = await CreateSut().Handle(
                MachineGroupBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MachineGroupBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MachineGroup>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(MachineGroupBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
