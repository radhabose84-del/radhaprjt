using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class MachineWiseFrequencyUpdateCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IBackgroundServiceClient> _mockBgClient = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);
        private readonly Mock<ILogger<MachineWiseFrequencyUpdateCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private MachineWiseFrequencyUpdateCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockQuery.Object, _mockBgClient.Object, _mockMisc.Object,
                _mockLogService.Object, _mockHttp.Object, _mockLogger.Object);

        private static PreventiveSchedulerDetail BuildDetail() => new()
        {
            PreventiveScheduler = null!,
            Machine = null!,
            Id = 1,
            FrequencyUnitId = 1
        };

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_InactivePath_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetPreventiveSchedulerDetailById(It.IsAny<int>()))
                .ReturnsAsync(BuildDetail());
            _mockMisc.Setup(m => m.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Code = "DAY" });

            var result = await CreateSut().Handle(
                new MachineWiseFrequencyUpdateCommand { Id = 1, IsActive = 0, FrequencyInterval = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ActivePath_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetPreventiveSchedulerDetailById(It.IsAny<int>()))
                .ReturnsAsync(BuildDetail());
            _mockMisc.Setup(m => m.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Code = "DAY" });
            _mockQuery.Setup(q => q.CalculateNextScheduleDate(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((DateTime.Today.AddDays(10), DateTime.Today.AddDays(5)));
            _mockQuery.Setup(q => q.ExistWorkOrderBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(
                new MachineWiseFrequencyUpdateCommand { Id = 1, IsActive = 1, FrequencyInterval = 1, LastMaintenanceActivityDate = DateOnly.FromDateTime(DateTime.Today) },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
