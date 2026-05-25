using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class CreateWOScheduleCommandValidatorTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateWOScheduleCommandValidator CreateValidator() => new(_mockRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_NullSchedule_FailsValidation()
        {
            var command = new CreateWOScheduleCommand { WOSchedule = null };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroWorkOrderId_FailsValidation()
        {
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto { WorkOrderId = 0 }
            };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UpdateWOScheduleCommandValidatorTests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateWOScheduleCommandValidator CreateValidator() => new(_mockRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_NullSchedule_FailsValidation()
        {
            var command = new UpdateWOScheduleCommand { WOSchedule = null };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullEndTime_FailsValidation()
        {
            var command = new UpdateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto { WorkOrderId = 1, EndTime = null }
            };
            // GetByIdAsync returns non-null — Loose mock default is fine since validator just checks != null

            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UpdateWorkOrderRequestDateCommandValidatorTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroWorkOrderId_FailsValidation()
        {
            var validator = new UpdateWorkOrderRequestDateCommandValidator(_mockQueryRepo.Object);
            var command = new UpdateWorkOrderRequestDateCommand { WorkOrderId = 0 };
            var result = await validator.TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        }

        [Fact]
        public async Task Validate_InvalidRequestDate_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.ValidateRequestDateAsync(1, It.IsAny<DateTimeOffset>(), 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var validator = new UpdateWorkOrderRequestDateCommandValidator(_mockQueryRepo.Object);
            var command = new UpdateWorkOrderRequestDateCommand
            {
                WorkOrderId = 1,
                RequestDate = DateTimeOffset.UtcNow.AddDays(-30),
                IsSystemTime = 0
            };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
