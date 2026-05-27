using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Presentation.Validation.DeliveryScoreRule;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.DeliveryScoreRule
{
    public sealed class CreateDeliveryScoreRuleCommandValidatorTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateDeliveryScoreRuleCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "DSR001")
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(code, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(ruleCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RuleCode);
        }

        [Theory]
        [InlineData("DSR-01")]
        [InlineData("DSR 01")]
        [InlineData("DSR@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(ruleCode: code);
            SetupAllAsyncMocks(code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RuleCode);
        }

        [Fact]
        public async Task Validate_CodeTooLong_FailsValidation()
        {
            var longCode = new string('A', 25);
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(ruleCode: longCode);
            SetupAllAsyncMocks(longCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RuleCode);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(ruleCode: "EXIST01");
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("EXIST01", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RuleCode);
        }

        [Fact]
        public async Task Validate_NegativeScore_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(score: -1m);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Score);
        }

        [Fact]
        public async Task Validate_NegativeDelayDaysFrom_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(delayDaysFrom: -1);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DelayDaysFrom);
        }

        [Fact]
        public async Task Validate_NegativeDelayDaysTo_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(delayDaysTo: -1);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DelayDaysTo);
        }

        [Fact]
        public async Task Validate_NegativeSortOrder_FailsValidation()
        {
            var command = DeliveryScoreRuleBuilders.ValidCreateCommand(sortOrder: -1);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SortOrder);
        }
    }
}
