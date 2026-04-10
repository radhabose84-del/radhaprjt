using BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Presentation.Validation.Workflow.ApprovalRules;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalRule
{
    public sealed class DeleteApprovalRuleCommandValidatorTests
    {
        private readonly Mock<IApprovalRuleQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteApprovalRuleCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalRuleCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // NotFound async rule still runs for Id=0
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalRuleCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalRuleCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
