using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Presentation.Validation.JournalMaster.RecurringJournalTemplate;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.RecurringJournalTemplate
{
    public sealed class CreateRecurringJournalTemplateCommandValidatorTests
    {
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateRecurringJournalTemplateCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.VoucherTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.FrequencyExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AmountAdjustmentRuleExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(RecurringTemplateBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_Fails(string? name)
        {
            SetupHappyPath();
            var cmd = RecurringTemplateBuilders.ValidCreateCommand();
            cmd.TemplateName = name;

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task Validate_DuplicateName_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.AlreadyExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(RecurringTemplateBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task Validate_NoLines_Fails()
        {
            SetupHappyPath();
            var cmd = RecurringTemplateBuilders.ValidCreateCommand(lines: new List<RecurringTemplateLineInputDto>());

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Lines);
        }

        [Fact]
        public async Task Validate_InvalidFrequency_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.FrequencyExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(RecurringTemplateBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.FrequencyId);
        }

        [Fact]
        public async Task Validate_EndDateBeforeStart_Fails()
        {
            SetupHappyPath();
            var cmd = RecurringTemplateBuilders.ValidCreateCommand();
            cmd.StartDate = new DateOnly(2026, 6, 1);
            cmd.EndDate = new DateOnly(2026, 4, 1);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_NonExistentAccount_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(RecurringTemplateBuilders.ValidCreateCommand());
            result.IsValid.Should().BeFalse();
        }
    }
}
