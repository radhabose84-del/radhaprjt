using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Validators.ProjectWorkBreakdownStructure
{
    public sealed class UpdateProjectWorkBreakdownStructureCommandValidatorTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureCommandRepository> _mockRepo =
            new(MockBehavior.Loose);

        private UpdateProjectWorkBreakdownStructureCommandValidator CreateValidator() =>
            new(_mockRepo.Object);

        private void SetupNameNotExists(int projectId, string name, int id) =>
            _mockRepo.Setup(r => r.NameExistsAsync(projectId, name, id)).ReturnsAsync(false);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand();
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(id: 0);
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, 0);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id is required.");
        }

        [Fact]
        public async Task Validate_ZeroProjectId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(projectId: 0);
            SetupNameNotExists(0, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ProjectId)
                  .WithErrorMessage("Project is required.");
        }

        [Fact]
        public async Task Validate_EmptyWbsName_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(wbsName: "");

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Fact]
        public async Task Validate_WbsNameTooLong_FailsValidation()
        {
            var longName = new string('A', 201);
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(wbsName: longName);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Fact]
        public async Task Validate_DuplicateWbsName_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(wbsName: "Duplicate WBS");
            _mockRepo.Setup(r => r.NameExistsAsync(cmd.ProjectId, "Duplicate WBS", cmd.Id))
                     .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName)
                  .WithErrorMessage("WBS Name must be unique within the same project.");
        }

        [Theory]
        [InlineData("WBS@Task")]
        [InlineData("WBS#Task")]
        [InlineData("WBS-Task")]
        public async Task Validate_InvalidWbsNameFormat_FailsValidation(string name)
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(wbsName: name);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.WorkBreakdownStructureName);
        }

        [Fact]
        public async Task Validate_ZeroCurrencyId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(currencyId: 0);
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("Currency is required.");
        }

        [Fact]
        public async Task Validate_EmptyResponsiblePerson_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand();
            cmd.ResponsiblePerson = "";
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ResponsiblePerson);
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand(departmentId: 0);
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleDepartmentId)
                  .WithErrorMessage("Responsible Department is required.");
        }

        [Fact]
        public async Task Validate_StartDateAfterEndDate_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand();
            cmd.StartDate = DateTimeOffset.UtcNow.AddDays(10);
            cmd.EndDate = DateTimeOffset.UtcNow;
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.StartDate)
                  .WithErrorMessage("Start Date cannot be greater than End Date.");
        }

        [Fact]
        public async Task Validate_MilestoneWithoutDate_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand();
            cmd.IsMilestone = true;
            cmd.MilestoneDate = null;
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MilestoneDate);
        }

        [Fact]
        public async Task Validate_NegativeBudgetAmount_FailsValidation()
        {
            var cmd = ProjectWorkBreakdownStructureBuilders.ValidUpdateCommand();
            cmd.PlannedBudgetAmount = -50m;
            SetupNameNotExists(cmd.ProjectId, cmd.WorkBreakdownStructureName, cmd.Id);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PlannedBudgetAmount);
        }
    }
}
