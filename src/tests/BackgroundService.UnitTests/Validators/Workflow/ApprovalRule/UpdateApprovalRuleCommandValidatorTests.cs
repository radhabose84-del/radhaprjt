using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.Workflow.ApprovalRules;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalRule
{
    public sealed class UpdateApprovalRuleCommandValidatorTests
    {
        private readonly Mock<IApprovalRuleQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateApprovalRuleCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private static UpdateApprovalRuleCommand ValidCommand() =>
            new()
            {
                Id = 1,
                ActionId = 1,
                ApprovalStepDetailId = 10,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                EffectiveTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Priority = 1,
                IsActive = 1,
                ApprovalRuleConditions = new List<ApprovalRuleConditionUpdateDto>()
            };

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
