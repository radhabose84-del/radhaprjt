using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.Workflow.ApprovalRules;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalRule
{
    public sealed class CreateApprovalRuleCommandValidatorTests
    {
        private readonly Mock<IApprovalRuleQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateApprovalRuleCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private static CreateApprovalRuleCommand ValidCommand() =>
            new()
            {
                ActionId = 1,
                ApprovalStepDetailId = 10,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Priority = 1,
                ApprovalRuleConditions = new List<ApprovalRuleConditionDto>()
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroActionId_FailsValidation()
        {
            var command = ValidCommand();
            command.ActionId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ActionId);
        }
    }
}
