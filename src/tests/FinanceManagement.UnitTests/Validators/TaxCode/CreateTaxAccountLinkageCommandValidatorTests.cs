using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Presentation.Validation.TaxCode;

namespace FinanceManagement.UnitTests.Validators.TaxCode
{
    public sealed class CreateTaxAccountLinkageCommandValidatorTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateTaxAccountLinkageCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo.Setup(r => r.TaxCodeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
        }

        private static CreateTaxAccountLinkageCommand ValidCommand() =>
            new() { TaxCodeId = 14, GlAccountId = 412, EffectiveFrom = new DateOnly(2026, 6, 16) };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullTaxCodeId_PassesValidation()
        {
            // TaxCodeId is optional — a linkage may carry no tax code.
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.TaxCodeId = null;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.TaxCodeId);
        }

        [Fact]
        public async Task Validate_NonExistentTaxCodeId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.TaxCodeExistsAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.TaxCodeId);
        }

        [Fact]
        public async Task Validate_NonExistentGlAccount_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.TaxCodeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(412)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.GlAccountId);
        }
    }
}
