using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class CreateWOScheduleCommandValidatorBatchATests
    {
        private readonly Mock<IWorkOrderCommandRepository> _mockRepo = new(MockBehavior.Loose);

        private CreateWOScheduleCommandValidator CreateValidator() => new(_mockRepo.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_NullWOSchedule_HasErrorForSchedule()
        {
            var command = new CreateWOScheduleCommand { WOSchedule = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WOSchedule);
        }

        [Fact]
        public async Task Validate_ScheduleWithNullWorkOrderId_FailsValidation()
        {
            var command = new CreateWOScheduleCommand
            {
                WOSchedule = new WorkOrderScheduleUpdateDto
                {
                    WorkOrderId = null,
                    StartTime = DateTimeOffset.UtcNow
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
