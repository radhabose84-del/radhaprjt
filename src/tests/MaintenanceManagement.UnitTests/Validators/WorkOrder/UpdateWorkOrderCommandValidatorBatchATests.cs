using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class UpdateWorkOrderCommandValidatorBatchATests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateWorkOrderCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_EmptyWorkOrder_FailsValidation()
        {
            var command = new UpdateWorkOrderCommand { WorkOrder = new WorkOrderUpdateDto() };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_WorkOrderWithZeroId_FailsValidation()
        {
            var command = new UpdateWorkOrderCommand
            {
                WorkOrder = new WorkOrderUpdateDto { Id = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
