using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage;
using FinanceManagement.Presentation.Validation.TaxCode;

namespace FinanceManagement.UnitTests.Validators.TaxCode
{
    public sealed class ActivateTaxAccountLinkageCommandValidatorTests
    {
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private ActivateTaxAccountLinkageCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_WithGlMapping_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.LinkageNotFoundAsync(3)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.LinkageHasGlMappingAsync(3)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new ActivateTaxAccountLinkageCommand(3));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithoutGlMapping_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.LinkageNotFoundAsync(3)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.LinkageHasGlMappingAsync(3)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new ActivateTaxAccountLinkageCommand(3));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Tax code cannot be activated without a GL account mapping.");
        }
    }
}
