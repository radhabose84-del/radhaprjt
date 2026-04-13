using FluentValidation.TestHelper;
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder.Item;
using MaintenanceManagement.Presentation.Validation.WorkOrder;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class UploadItemCommandValidatorBatchATests
    {
        private UploadItemCommandValidator CreateValidator() => new();

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_NullFile_HasErrorForFile()
        {
            var command = new UploadFileItemCommand { File = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }

        [Fact]
        public async Task Validate_NullFile_ErrorsNotEmpty()
        {
            var command = new UploadFileItemCommand { File = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
