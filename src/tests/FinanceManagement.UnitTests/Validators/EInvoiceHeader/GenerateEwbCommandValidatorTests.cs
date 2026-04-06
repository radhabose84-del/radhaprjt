using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using FinanceManagement.Presentation.Validation.EInvoiceHeader;

namespace FinanceManagement.UnitTests.Validators.EInvoiceHeader
{
    public sealed class GenerateEwbCommandValidatorTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private GenerateEwbCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new GenerateEwbCommand { EInvoiceHeaderId = 1, Distance = 10 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroEInvoiceHeaderId_FailsValidation()
        {
            var command = new GenerateEwbCommand { EInvoiceHeaderId = 0, Distance = 10 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EInvoiceHeaderId);
        }

        [Fact]
        public async Task Validate_NotFoundEInvoiceHeaderId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new GenerateEwbCommand { EInvoiceHeaderId = 99, Distance = 10 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EInvoiceHeaderId);
        }

        [Fact]
        public async Task Validate_ZeroDistance_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new GenerateEwbCommand { EInvoiceHeaderId = 1, Distance = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Distance);
        }

        [Fact]
        public async Task Validate_NegativeDistance_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new GenerateEwbCommand { EInvoiceHeaderId = 1, Distance = -5 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Distance);
        }
    }
}
