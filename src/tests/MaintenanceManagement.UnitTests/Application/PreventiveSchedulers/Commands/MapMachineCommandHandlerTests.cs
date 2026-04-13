using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class MapMachineCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IBackgroundServiceClient> _mockBgClient = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);
        private readonly Mock<ILogger<MapMachineCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private MapMachineCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockMapper.Object, _mockQuery.Object, _mockMisc.Object,
                _mockBgClient.Object, _mockLogService.Object, _mockHttp.Object, _mockLogger.Object);

        private static PreventiveSchedulerHeader BuildHeader() => new()
        {
            MachineGroup = null!,
            MiscMaintenanceCategory = null!,
            MiscSchedule = null!,
            MiscFrequencyType = null!,
            MiscFrequencyUnit = null!,
            Id = 1,
            FrequencyUnitId = 1,
            FrequencyInterval = 1,
            ReminderWorkOrderDays = 1,
            ReminderMaterialReqDays = 1
        };

        private static PreventiveSchedulerDetail BuildDetail() => new()
        {
            PreventiveScheduler = null!,
            Machine = null!,
            Id = 1,
            WorkOrderCreationStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
        };

        [Fact]
        public async Task Handle_HappyPath_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(BuildHeader());
            _mockMisc.Setup(m => m.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Code = "DAY" });
            _mockQuery.Setup(q => q.CalculateNextScheduleDate(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((DateTime.Today.AddDays(10), DateTime.Today.AddDays(5)));
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerDetail>(It.IsAny<object>()))
                .Returns(BuildDetail());
            _mockCommand.Setup(c => c.CreateDetailAsync(It.IsAny<PreventiveSchedulerDetail>()))
                .ReturnsAsync(BuildDetail());

            var result = await CreateSut().Handle(
                new MapMachineCommand { Id = 1, MachineId = 10, LastMaintenanceActivityDate = DateOnly.FromDateTime(DateTime.Today) },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
