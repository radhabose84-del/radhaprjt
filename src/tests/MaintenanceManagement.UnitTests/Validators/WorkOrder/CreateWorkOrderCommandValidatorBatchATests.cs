using FluentValidation.TestHelper;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class CreateWorkOrderCommandValidatorBatchATests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateWorkOrderCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public void Constructor_WithValidProvider_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_BothRequestAndPreventiveIdsNull_FailsValidation()
        {
            var command = new CreateWorkOrderCommand
            {
                WorkOrderDto = new WorkOrderCombineDto
                {
                    RequestId = null,
                    PreventiveScheduleId = null
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_BothRequestAndPreventiveIdsProvided_FailsValidation()
        {
            var command = new CreateWorkOrderCommand
            {
                WorkOrderDto = new WorkOrderCombineDto
                {
                    RequestId = 1,
                    PreventiveScheduleId = 2
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_OnlyRequestIdProvided_PassesExclusivityRule()
        {
            var command = new CreateWorkOrderCommand
            {
                WorkOrderDto = new WorkOrderCombineDto
                {
                    RequestId = 10,
                    PreventiveScheduleId = null
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            // exclusivity rule satisfied; there may still be other errors but not for WorkOrderDto exclusivity
            result.Errors.Should()
                .NotContain(e => e.ErrorMessage.Contains("Either RequestId or PreventiveScheduleId"));
        }
    }
}
