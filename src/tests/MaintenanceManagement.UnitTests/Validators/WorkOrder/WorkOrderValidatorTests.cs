using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.DeleteFileWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class CreateWorkOrderCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        [Fact]
        public async Task Validate_EmptyRequestTypeId_FailsValidation()
        {
            var validator = new CreateWorkOrderCommandValidator(_mockMaxLength.Object);
            var command = new CreateWorkOrderCommand { WorkOrderDto = new WorkOrderCombineDto() };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UpdateWorkOrderCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var validator = new UpdateWorkOrderCommandValidator(_mockMaxLength.Object, _mockCommandRepo.Object, _mockQueryRepo.Object);
            var command = new UpdateWorkOrderCommand { WorkOrder = new WorkOrderUpdateDto() };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UploadWorkOrderCommandValidatorTests
    {
        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var validator = new UploadWorkOrderCommandValidator();
            var command = new UploadFileWorkOrderCommand { File = null };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UploadItemCommandValidatorTests
    {
        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var validator = new UploadItemCommandValidator();
            var command = new UploadFileItemCommand { File = null };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
