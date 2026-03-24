using FluentValidation.TestHelper;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using ProjectManagement.Presentation.Validation.ProjectWorkBreakdownStructure;

namespace ProjectManagement.UnitTests.Validators.ProjectWorkBreakdownStructure
{
    public sealed class DeleteProjectWBSCommandValidatorTests
    {
        private static DeleteProjectWorkBreakdownStructureCommandValidator CreateValidator() =>
            new();

        [Fact]
        public void Validate_ValidId_PassesValidation()
        {
            var result = CreateValidator().TestValidate(new DeleteProjectWorkBreakdownStructureCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_ZeroId_FailsValidation()
        {
            var result = CreateValidator().TestValidate(new DeleteProjectWorkBreakdownStructureCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id must be greater than zero.");
        }

        [Fact]
        public void Validate_NegativeId_FailsValidation()
        {
            var result = CreateValidator().TestValidate(new DeleteProjectWorkBreakdownStructureCommand(-1));
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id must be greater than zero.");
        }
    }
}
