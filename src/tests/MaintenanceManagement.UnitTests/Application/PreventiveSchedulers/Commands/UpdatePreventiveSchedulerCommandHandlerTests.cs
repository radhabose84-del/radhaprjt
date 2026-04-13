using AutoMapper;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class UpdatePreventiveSchedulerCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockWorkOrder = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceUnitOfWork> _mockUow = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdatePreventiveSchedulerCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdatePreventiveSchedulerCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockMapper.Object, _mockMediator.Object, _mockQuery.Object,
                _mockWorkOrder.Object, _mockIp.Object, _mockTz.Object, _mockUow.Object,
                _mockOutbox.Object, _mockLogService.Object, _mockHttp.Object, _mockMisc.Object,
                _mockLogger.Object);

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            Action act = () => { _ = CreateSut(); };
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Handle_HappyPath_ReturnsSuccess()
        {
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHeader>(It.IsAny<object>()))
                .Returns(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null! });
            _mockQuery.Setup(q => q.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null! });
            _mockCommand.Setup(c => c.UpdateScheduleMetadata(It.IsAny<PreventiveSchedulerHeader>()))
                .ReturnsAsync(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null!, Id = 1, FrequencyUnitId = 1 });
            _mockMisc.Setup(m => m.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Code = "DAY" });
            _mockQuery.Setup(q => q.GetPreventiveSchedulerDetail(It.IsAny<int>()))
                .ReturnsAsync(new List<PreventiveSchedulerDetail>());
            _mockCommand.Setup(c => c.UpdateScheduleDetails(It.IsAny<int>(), It.IsAny<List<PreventiveSchedulerDetail>>()))
                .ReturnsAsync(new List<PreventiveSchedulerDetail>());

            var result = await CreateSut().Handle(new UpdatePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenUpdateReturnsNull_ReturnsFailure()
        {
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerHeader>(It.IsAny<object>()))
                .Returns(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null! });
            _mockQuery.Setup(q => q.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null! });
            _mockCommand.Setup(c => c.UpdateScheduleMetadata(It.IsAny<PreventiveSchedulerHeader>()))
                .ReturnsAsync(new PreventiveSchedulerHeader { MachineGroup = null!, MiscMaintenanceCategory = null!, MiscSchedule = null!, MiscFrequencyType = null!, MiscFrequencyUnit = null!, Id = 0 });

            var result = await CreateSut().Handle(new UpdatePreventiveSchedulerCommand { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
