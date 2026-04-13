using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class ActiveInActivePreventiveCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMachineMasterQueryRepository> _mockMachineRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockWorkOrderRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IBackgroundServiceClient> _mockBgClient = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);

        private ActiveInActivePreventiveCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockMediator.Object, _mockQuery.Object, _mockMachineRepo.Object,
                _mockMiscRepo.Object, _mockWorkOrderRepo.Object, _mockMapper.Object, _mockBgClient.Object,
                _mockLogService.Object, _mockHttp.Object);

        private static PreventiveSchedulerHeader BuildHeader() => new()
        {
            MachineGroup = null!,
            MiscMaintenanceCategory = null!,
            MiscSchedule = null!,
            MiscFrequencyType = null!,
            MiscFrequencyUnit = null!,
            Id = 1,
            FrequencyUnitId = 1,
            PreventiveSchedulerDetails = new List<PreventiveSchedulerDetail>(),
            PreventiveSchedulerActivities = new List<PreventiveSchedulerActivity>(),
            PreventiveSchedulerItems = new List<PreventiveSchedulerItems>()
        };

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_InactivePath_ReturnsTrue()
        {
            _mockQuery.Setup(q => q.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(BuildHeader());
            _mockQuery.Setup(q => q.WorkOrderNotGeneratedScheduler(It.IsAny<int>()))
                .ReturnsAsync(new List<int>());
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHeader>(It.IsAny<object>()))
                .Returns(BuildHeader());

            var result = await CreateSut().Handle(
                new ActiveInActivePreventiveCommand { Id = 1, IsActive = 0 }, CancellationToken.None);

            result.Should().BeTrue();
        }
    }
}
