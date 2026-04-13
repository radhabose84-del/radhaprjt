using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Commands
{
    public sealed class CreateWOScheduleCommandHandlerTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private CreateWOScheduleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockTimeZone.Object);

        private void SetupHappyPath(int resultId = 1)
        {
            _mockTimeZone.Setup(t => t.GetSystemTimeZone()).Returns("UTC");

            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>(It.IsAny<object>()))
                .Returns(new MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule { WorkOrderId = 1, MiscStatus = null! });

            _mockCommandRepo.Setup(r => r.CreateScheduleAsync(
                    It.IsAny<int>(),
                    It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>()))
                .ReturnsAsync(resultId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(resultId: 5);
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 1
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ReturnsFailure()
        {
            SetupHappyPath(resultId: 0);
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 1
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CallsCreateScheduleAsyncOnce()
        {
            SetupHappyPath();
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    IsSystemTime = 0
                }
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateScheduleAsync(
                It.IsAny<int>(),
                It.IsAny<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonSystemTime_HandlesCorrectly()
        {
            SetupHappyPath();
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = 1,
                    StartTime = DateTimeOffset.UtcNow,
                    EndTime = DateTimeOffset.UtcNow.AddHours(1),
                    IsSystemTime = 0
                }
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
