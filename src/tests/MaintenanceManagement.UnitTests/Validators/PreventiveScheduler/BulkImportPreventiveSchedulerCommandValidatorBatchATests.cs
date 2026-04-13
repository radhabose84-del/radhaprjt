using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class BulkImportPreventiveSchedulerCommandValidatorBatchATests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        private BulkImportPreventiveSchedulerCommandValidator CreateValidator() =>
            new(_mockQuery.Object);

        [Fact]
        public void Constructor_WithValidQuery_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var command = new RescheduleBulkImportCommand { File = null! };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullFile_HasErrorForFile()
        {
            var command = new RescheduleBulkImportCommand { File = null! };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.File);
        }
    }
}
