using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ReschedulePreventive;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Commands.BatchD
{
    public sealed class ReschedulePreventiveCommandHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockLogService = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ReschedulePreventiveCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private ReschedulePreventiveCommandHandler CreateSut() =>
            new(_mockCommand.Object, _mockMapper.Object, _mockMediator.Object, _mockMisc.Object,
                _mockQuery.Object, _mockLogService.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithExistingWorkOrder_UpdatesRescheduleDate()
        {
            _mockQuery.Setup(q => q.ExistWorkOrderBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockQuery.Setup(q => q.GetClosedDateBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync((DateOnly?)null);

            var result = await CreateSut().Handle(
                new ReshedulePreventiveCommand { PreventiveScheduleDetailId = 1, RescheduleDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)) },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithoutWorkOrder_CallsRescheduleWithoutWorkOrderAsync()
        {
            _mockQuery.Setup(q => q.ExistWorkOrderBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQuery.Setup(q => q.GetClosedDateBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync((DateOnly?)null);

            var result = await CreateSut().Handle(
                new ReshedulePreventiveCommand { PreventiveScheduleDetailId = 1, RescheduleDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SameAsClosedDate_ReturnsFailure()
        {
            var closedDate = DateOnly.FromDateTime(DateTime.Today);
            _mockQuery.Setup(q => q.GetClosedDateBySchedulerDetailId(It.IsAny<int>()))
                .ReturnsAsync(closedDate);

            var result = await CreateSut().Handle(
                new ReshedulePreventiveCommand { PreventiveScheduleDetailId = 1, RescheduleDate = closedDate },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
