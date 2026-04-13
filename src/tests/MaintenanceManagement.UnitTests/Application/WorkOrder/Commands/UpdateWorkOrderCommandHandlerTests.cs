using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UpdateWorkOrderCommandHandlerTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceUnitOfWork> _mockUow = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateWorkOrderCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<ILogQueryService> _mockLogQueryService = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveScheduleLogService> _mockPreventiveLogService = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerCommand> _mockPreventiveSchedulerCommand = new(MockBehavior.Loose);

        private UpdateWorkOrderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object,
                _mockUow.Object, _mockOutbox.Object, _mockLogger.Object, _mockLogQueryService.Object,
                _mockUnitLookup.Object, _mockCompanyLookup.Object, _mockHttpContextAccessor.Object,
                _mockTimeZoneService.Object, _mockPreventiveLogService.Object, _mockPreventiveSchedulerCommand.Object);

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockPreventiveLogService.Setup(l => l.CaptureLogs(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>(It.IsAny<object>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder { Id = 1, StatusId = 10, MiscStatus = null! });

            _mockCommandRepo.Setup(r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>()))
                .ReturnsAsync(updateResult);

            _mockCommandRepo.Setup(r => r.GetMiscMasterByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Id = 99 });

            _mockUow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockUow.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var command = new UpdateWorkOrderCommand
            {
                WorkOrder = new WorkOrderUpdateDto { Id = 1, CompanyId = 1, UnitId = 1 }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            SetupHappyPath(updateResult: false);
            var command = new UpdateWorkOrderCommand
            {
                WorkOrder = new WorkOrderUpdateDto { Id = 1, CompanyId = 1, UnitId = 1 }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_BeginsAndCommitsTransaction()
        {
            SetupHappyPath();
            var command = new UpdateWorkOrderCommand
            {
                WorkOrder = new WorkOrderUpdateDto { Id = 1, CompanyId = 1, UnitId = 1 }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockUow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsCaptureLogsOnce()
        {
            SetupHappyPath();
            var command = new UpdateWorkOrderCommand
            {
                WorkOrder = new WorkOrderUpdateDto { Id = 1, CompanyId = 1, UnitId = 1 }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockPreventiveLogService.Verify(l => l.CaptureLogs(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
