using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.ProjectWorkBreakdownStructure
{
    public sealed class CreateProjectWBSCommandValidatorTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockRepo =
            new(MockBehavior.Strict);

        private CreateProjectWorkBreakdownStructureCommandValidator CreateValidator() =>
            new(_mockRepo.Object);

        private void SetupNameNotExists(int projectId, string name) =>
            _mockRepo.Setup(r => r.NameExistsAsync(projectId, name)).ReturnsAsync(false);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand();
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroProjectId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(projectId: 0);
            SetupNameNotExists(0, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ProjectId)
                  .WithErrorMessage("Project is required.");
        }

        [Fact]
        public async Task Validate_EmptyWbsName_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(wbsName: "");

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Fact]
        public async Task Validate_WbsNameTooLong_FailsValidation()
        {
            var longName = new string('A', 201);
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(wbsName: longName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Theory]
        [InlineData("WBS@Task")]
        [InlineData("WBS#Task")]
        [InlineData("WBS-Task")]
        public async Task Validate_InvalidWbsNameFormat_FailsValidation(string name)
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(wbsName: name);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Fact]
        public async Task Validate_DuplicateWbsName_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(wbsName: "Duplicate WBS");
            _mockRepo.Setup(r => r.NameExistsAsync(cmd.ProjectId, "Duplicate WBS")).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName)
                  .WithErrorMessage("WBS Name must be unique within the same project.");
        }

        [Fact]
        public async Task Validate_ZeroCurrencyId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(currencyId: 0);
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("Currency is required.");
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(departmentId: 0);
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleDepartmentId)
                  .WithErrorMessage("Responsible Department is required.");
        }

        [Fact]
        public async Task Validate_EmptyResponsiblePerson_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand(responsiblePerson: "");
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ResponsiblePerson);
        }

        [Fact]
        public async Task Validate_StartDateAfterEndDate_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand();
            cmd.StartDate = DateTimeOffset.UtcNow.AddDays(10);
            cmd.EndDate = DateTimeOffset.UtcNow;
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.StartDate)
                  .WithErrorMessage("Start Date cannot be greater than End Date.");
        }

        [Fact]
        public async Task Validate_MilestoneWithoutDate_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidCreateCommand();
            cmd.IsMilestone = true;
            cmd.MilestoneDate = null;
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MilestoneDate);
        }
    }
}
