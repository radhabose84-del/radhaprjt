using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment;
using SalesManagement.Presentation.Validation.ProformaInvoice;

namespace SalesManagement.UnitTests.Validators.ProformaInvoice
{
    public sealed class UpdateProformaPaymentCommandValidatorTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateProformaPaymentCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private static UpdateProformaPaymentCommand ValidCommand(
            int id = 1,
            decimal paymentReceivedAmount = 5000m)
        {
            return new UpdateProformaPaymentCommand
            {
                Id = id,
                PaymentReceivedAmount = paymentReceivedAmount
            };
        }

        private void SetupAllAsyncMocksPass(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(id)).ReturnsAsync(10000m);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task Id_Zero_FailsNotEmpty()
        {
            var command = ValidCommand(id: 0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = ValidCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(99)).ReturnsAsync(10000m);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── PaymentReceivedAmount Rules ───────────────────────────────────────

        [Fact]
        public async Task PaymentReceivedAmount_Negative_FailsValidation()
        {
            var command = ValidCommand(paymentReceivedAmount: -100m);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PaymentReceivedAmount);
        }

        [Fact]
        public async Task PaymentReceivedAmount_Zero_PassesValidation()
        {
            var command = ValidCommand(paymentReceivedAmount: 0m);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.PaymentReceivedAmount);
        }

        [Fact]
        public async Task PaymentReceivedAmount_ExceedsProformaAmount_FailsValidation()
        {
            var command = ValidCommand(paymentReceivedAmount: 15000m);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.GetProformaAmountAsync(1)).ReturnsAsync(10000m);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task PaymentReceivedAmount_EqualToProformaAmount_PassesValidation()
        {
            var command = ValidCommand(paymentReceivedAmount: 10000m);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
