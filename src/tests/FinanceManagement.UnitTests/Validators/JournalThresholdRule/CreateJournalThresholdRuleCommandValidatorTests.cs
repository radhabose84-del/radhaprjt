using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Presentation.Validation.JournalMaster.JournalThresholdRule;
using FinanceManagement.UnitTests.TestData;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.JournalThresholdRule
{
    public sealed class CreateJournalThresholdRuleCommandValidatorTests
    {
        private readonly Mock<IJournalThresholdRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateJournalThresholdRuleCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupHappyPath() =>
            _mockQueryRepo.Setup(r => r.RuleTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(JournalThresholdRuleBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroRuleType_Fails()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(JournalThresholdRuleBuilders.ValidCreateCommand(ruleTypeId: 0));
            result.ShouldHaveValidationErrorFor(x => x.RuleTypeId);
        }

        [Fact]
        public async Task Validate_InvalidRuleType_Fails()
        {
            _mockQueryRepo.Setup(r => r.RuleTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(JournalThresholdRuleBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.RuleTypeId);
        }

        [Fact]
        public async Task Validate_NegativeThreshold_Fails()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(JournalThresholdRuleBuilders.ValidCreateCommand(threshold: -1m));
            result.IsValid.Should().BeFalse();
        }
    }
}
