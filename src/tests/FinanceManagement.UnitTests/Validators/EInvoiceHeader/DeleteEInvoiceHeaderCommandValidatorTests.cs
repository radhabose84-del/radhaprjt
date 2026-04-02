using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader;
using FinanceManagement.Presentation.Validation.EInvoiceHeader;

namespace FinanceManagement.UnitTests.Validators.EInvoiceHeader
{
    public sealed class DeleteEInvoiceHeaderCommandValidatorTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteEInvoiceHeaderCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteEInvoiceHeaderCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeleteEInvoiceHeaderCommand(99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new DeleteEInvoiceHeaderCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
