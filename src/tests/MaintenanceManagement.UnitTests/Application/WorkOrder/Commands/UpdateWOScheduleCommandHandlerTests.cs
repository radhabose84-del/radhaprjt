using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class UpdateWOScheduleCommandHandlerTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private UpdateWOScheduleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockTimeZone.Object);

        private void SetupHappyPath(bool updateResult = true)
        {
            _mockTimeZone.Setup(t => t.GetSystemTimeZone()).Returns("UTC");

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>(It.IsAny<object>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule { WorkOrderId = 1, MiscStatus = null! });

            _mockCommandRepo.Setup(r => r.UpdateScheduleAsync(
                    It.IsAny<int>(),
                    It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>()))
                .ReturnsAsync(updateResult);
        }

        [Fact]
        public async Task Handle_NullSchedule_ReturnsFailure()
        {
            var command = new UpdateWOScheduleCommand { WOSchedule = null };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("missing");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var command = new UpdateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 0
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFails_ReturnsFailure()
        {
            SetupHappyPath(updateResult: false);
            var command = new UpdateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 0
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsUpdateScheduleAsyncOnce()
        {
            SetupHappyPath();
            var command = new UpdateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 1
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateScheduleAsync(
                It.IsAny<int>(),
                It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>()),
                Times.Once);
        }
    }
}
