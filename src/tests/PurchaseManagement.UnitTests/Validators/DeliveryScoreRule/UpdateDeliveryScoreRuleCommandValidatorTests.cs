using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Presentation.Validation.DeliveryScoreRule;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.DeliveryScoreRule
{
    public sealed class UpdateDeliveryScoreRuleCommandValidatorTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateDeliveryScoreRuleCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NegativeScore_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(score: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Score);
        }

        [Fact]
        public async Task Validate_NegativeDelayDaysFrom_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(delayDaysFrom: -1);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DelayDaysFrom);
        }

        [Fact]
        public async Task Validate_NegativeDelayDaysTo_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(delayDaysTo: -1);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DelayDaysTo);
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
