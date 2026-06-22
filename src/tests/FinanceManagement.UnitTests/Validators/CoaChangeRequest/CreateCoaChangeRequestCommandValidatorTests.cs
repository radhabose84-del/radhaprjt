using FluentValidation.TestHelper;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest;
using FinanceManagement.Presentation.Validation.CoaChangeRequest;

namespace FinanceManagement.UnitTests.Validators.CoaChangeRequest
{
    public sealed class CreateCoaChangeRequestCommandValidatorTests
    {
        private readonly CreateCoaChangeRequestCommandValidator _validator = new();

        private static CreateCoaChangeRequestCommand Valid() => new()
        {
            TargetAccountId = 5,
            ChangeType = "AccountEdit",
            Justification = "year-end correction",
            ImpactAssessment = "no downstream impact"
        };

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            var result = await _validator.TestValidateAsync(Valid());
            result.ShouldNotHaveAnyValidationErrors();
        }

        // AC5 — impact assessment is mandatory.
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_MissingImpactAssessment_Fails(string? impact)
        {
            var cmd = Valid();
            cmd.ImpactAssessment = impact!;
            var result = await _validator.TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ImpactAssessment);
        }

        [Fact]
        public async Task Validate_NoTarget_Fails()
        {
            var cmd = Valid();
            cmd.TargetAccountId = null;
            cmd.TargetAccountGroupId = null;
            var result = await _validator.TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public async Task Validate_MissingJustification_Fails()
        {
            var cmd = Valid();
            cmd.Justification = "";
            var result = await _validator.TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Justification);
        }
    }
}
