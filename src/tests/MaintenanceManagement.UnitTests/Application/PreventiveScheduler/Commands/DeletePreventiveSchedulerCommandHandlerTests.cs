using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.PreventiveScheduler.Commands
{
    public sealed class DeletePreventiveSchedulerCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeletePreventiveSchedulerCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHeader>(It.IsAny<DeletePreventiveSchedulerCommand>()))
                .Returns(new PreventiveSchedulerHeader
                {
                    MachineGroup = new MaintenanceManagement.Domain.Entities.MachineGroup(),
                    MiscMaintenanceCategory = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscSchedule = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscFrequencyType = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscFrequencyUnit = new MaintenanceManagement.Domain.Entities.MiscMaster()
                });
            _mockCommandRepo.Setup(r => r.DeleteAsync(id, It.IsAny<PreventiveSchedulerHeader>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetPreventiveSchedulerDetail(id)).ReturnsAsync(new List<PreventiveSchedulerDetail>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new DeletePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeletePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);
            _mockCommandRepo.Verify(r => r.DeleteAsync(1, It.IsAny<PreventiveSchedulerHeader>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeletePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteReturnsFalse_ThrowsExceptionRules()
        {
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHeader>(It.IsAny<DeletePreventiveSchedulerCommand>()))
                .Returns(new PreventiveSchedulerHeader
                {
                    MachineGroup = new MaintenanceManagement.Domain.Entities.MachineGroup(),
                    MiscMaintenanceCategory = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscSchedule = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscFrequencyType = new MaintenanceManagement.Domain.Entities.MiscMaster(),
                    MiscFrequencyUnit = new MaintenanceManagement.Domain.Entities.MiscMaster()
                });
            _mockCommandRepo.Setup(r => r.DeleteAsync(1, It.IsAny<PreventiveSchedulerHeader>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetPreventiveSchedulerDetail(1)).ReturnsAsync(new List<PreventiveSchedulerDetail>());

            Func<Task> act = async () => await CreateSut().Handle(new DeletePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
