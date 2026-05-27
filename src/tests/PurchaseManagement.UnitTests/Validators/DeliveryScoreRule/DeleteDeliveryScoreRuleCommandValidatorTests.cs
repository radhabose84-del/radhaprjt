using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule;
using PurchaseManagement.Presentation.Validation.DeliveryScoreRule;

namespace PurchaseManagement.UnitTests.Validators.DeliveryScoreRule
{
    public sealed class DeleteDeliveryScoreRuleCommandValidatorTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteDeliveryScoreRuleCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryScoreRuleCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryScoreRuleCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteDeliveryScoreRuleCommand(99));

            result.ShouldHaveAnyValidationError();
        }
    }
}
