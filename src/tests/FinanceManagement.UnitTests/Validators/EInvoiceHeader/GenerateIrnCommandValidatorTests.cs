using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using FinanceManagement.Presentation.Validation.EInvoiceHeader;

namespace FinanceManagement.UnitTests.Validators.EInvoiceHeader
{
    public sealed class GenerateIrnCommandValidatorTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private GenerateIrnCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new GenerateIrnCommand { EInvoiceHeaderId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroEInvoiceHeaderId_FailsValidation()
        {
            var command = new GenerateIrnCommand { EInvoiceHeaderId = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EInvoiceHeaderId);
        }

        [Fact]
        public async Task Validate_NotFoundEInvoiceHeaderId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new GenerateIrnCommand { EInvoiceHeaderId = 99 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EInvoiceHeaderId);
        }
    }
}
