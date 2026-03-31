using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using ProjectManagement.Presentation.Validation.ProjectMaster;
using ProjectManagement.UnitTests.TestData;
using Shared.Validation.Common;

namespace ProjectManagement.UnitTests.Validators.ProjectMaster
{
    public sealed class CreateProjectMasterCommandValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private CreateProjectMasterCommandValidator CreateValidator(
            List<ValidationRule>? rules = null) =>
            new(_mockWorkflowLookup.Object, rules ?? new List<ValidationRule>());

        public CreateProjectMasterCommandValidatorTests()
        {
            _mockWorkflowLookup
                .Setup(l => l.IsApproveWorkflowConfigureAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyProjectName_FailsValidation(string? name)
        {
            var command = ProjectMasterBuilders.ValidCreateCommand(projectName: name!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.ProjectName)
                  .WithErrorMessage("Project Name is required.");
        }

        [Fact]
        public async Task Validate_ProjectNameTooLong_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand(projectName: new string('A', 201));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.ProjectName)
                  .WithErrorMessage("Project Name cannot exceed 200 characters.");
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand(unitId: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.UnitId)
                  .WithErrorMessage("Unit is required.");
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand(departmentId: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.DepartmentId)
                  .WithErrorMessage("Department is required.");
        }

        [Fact]
        public async Task Validate_NegativeBudgetAmount_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand();
            command.Project.BudgetAmount = -1m;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.BudgetAmount)
                  .WithErrorMessage("Budget Amount cannot be negative.");
        }

        [Fact]
        public async Task Validate_EndDateBeforeStartDate_FailsValidation()
        {
            var command = ProjectMasterBuilders.ValidCreateCommand();
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
            var command = ProjectMasterBuilders.ValidCreateCommand();
            command.Project.PurposeRemarks = remarks!;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Project.PurposeRemarks)
                  .WithErrorMessage("Purpose / Remarks is required.");
        }

        [Fact]
        public async Task Validate_WorkflowRule_WhenConfigured_IsChecked()
        {
            var workflowRule = new ValidationRule { Rule = "Workflow", Error = "Workflow must be configured." };
            _mockWorkflowLookup
                .Setup(l => l.IsApproveWorkflowConfigureAsync(
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var command = ProjectMasterBuilders.ValidCreateCommand();
            var result = await CreateValidator(new List<ValidationRule> { workflowRule })
                .TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Project.UnitId)
                  .WithErrorMessage("Workflow must be configured.");
        }
    }
}
