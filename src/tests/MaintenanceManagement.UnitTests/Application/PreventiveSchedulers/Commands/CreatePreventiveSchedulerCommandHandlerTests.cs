using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class CreatePreventiveSchedulerCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceUnitOfWork> _mockUnitOfWork = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<IMachineMasterQueryRepository> _mockMachineRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreatePreventiveSchedulerCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreatePreventiveSchedulerCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockMediator.Object, _mockOutbox.Object, _mockUnitOfWork.Object,
                _mockIp.Object, _mockLogService.Object, _mockMachineRepo.Object, _mockMiscRepo.Object,
                _mockQuery.Object, _mockLogger.Object);

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_NoMachinesFound_ThrowsExceptionRules()
        {
            _mockMachineRepo.Setup(r => r.GetMachineByGroupSagaAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster>());

            Func<Task> act = async () => await CreateSut().Handle(new CreatePreventiveSchedulerCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_InvalidFrequencyUnit_ThrowsExceptionRules()
        {
            _mockMachineRepo.Setup(r => r.GetMachineByGroupSagaAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineMaster> { new() { Id = 1 } });
            _mockMiscRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Code = null });

            Func<Task> act = async () => await CreateSut().Handle(new CreatePreventiveSchedulerCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
