using FluentValidation.TestHelper;
using ProjectManagement.Presentation.Validation.ProjectMaster;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.ProjectMaster
{
    public sealed class UpdateProjectMasterCommandValidatorTests
    {
        private static UpdateProjectMasterCommandValidator CreateValidator() =>
            new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand(id: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.Id)
                  .WithErrorMessage("Id is required.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyProjectName_FailsValidation(string? name)
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand(projectName: name!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.ProjectName)
                  .WithErrorMessage("Project Name is required.");
        }

        [Fact]
        public async Task Validate_ProjectNameTooLong_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand(projectName: new string('A', 201));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.ProjectName)
                  .WithErrorMessage("Project Name cannot exceed 200 characters.");
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            command.Project.UnitId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.UnitId)
                  .WithErrorMessage("Unit is required.");
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            command.Project.DepartmentId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.DepartmentId)
                  .WithErrorMessage("Department is required.");
        }

        [Fact]
        public async Task Validate_NegativeBudgetAmount_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            command.Project.BudgetAmount = -1m;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.BudgetAmount)
                  .WithErrorMessage("Budget Amount cannot be negative.");
        }

        [Fact]
        public async Task Validate_EndDateBeforeStartDate_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            command.Project.StartDate = DateTimeOffset.UtcNow.AddDays(10);
            command.Project.EndDate = DateTimeOffset.UtcNow;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.EndDate)
                  .WithErrorMessage("End Date must be greater than or equal to Start Date.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPurposeRemarks_FailsValidation(string? remarks)
        {
            var command = ProjectMasterBuilders.ValidUpdateCommand();
            command.Project.PurposeRemarks = remarks!;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.PurposeRemarks)
                  .WithErrorMessage("Purpose / Remarks is required.");
        }
    }
}
