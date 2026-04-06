using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Presentation.Validation.EInvoiceHeader;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.EInvoiceHeader
{
    public sealed class CreateEInvoiceHeaderCommandValidatorTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateEInvoiceHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string? irnNumber = null)
        {
            if (irnNumber != null)
                _mockQueryRepo.Setup(r => r.IrnNumberExistsAsync(irnNumber, null)).ReturnsAsync(false);
        }

        private static CreateEInvoiceHeaderCommand ValidCommand() =>
            new()
            {
                UnitId = 1,
                PartyId = 1,
                InvoiceNo = "INV001",
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                IrnNumber = "IRN001",
                AckNo = "ACK001",
                GstNo = "22AAAAA1234A1Z5",
                CGST = 100m,
                SGST = 100m,
                IGST = 0m,
                TCS = 0m,
                Discount = 0m,
                Cess = 0m,
                OtherCharges = 0m
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_ZeroPartyId_FailsValidation()
        {
            var command = ValidCommand();
            command.PartyId = 0;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyInvoiceNo_FailsValidation(string? invoiceNo)
        {
            var command = ValidCommand();
            command.InvoiceNo = invoiceNo;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceNo);
        }

        [Fact]
        public async Task Validate_InvoiceNoExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.InvoiceNo = new string('A', 31);
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceNo);
        }

        [Fact]
        public async Task Validate_DuplicateIrnNumber_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.IrnNumberExistsAsync("IRN001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IrnNumber);
        }

        [Fact]
        public async Task Validate_NegativeCGST_FailsValidation()
        {
            var command = ValidCommand();
            command.CGST = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CGST);
        }

        [Fact]
        public async Task Validate_NegativeSGST_FailsValidation()
        {
            var command = ValidCommand();
            command.SGST = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SGST);
        }

        [Fact]
        public async Task Validate_NegativeIGST_FailsValidation()
        {
            var command = ValidCommand();
            command.IGST = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IGST);
        }

        [Fact]
        public async Task Validate_NegativeDiscount_FailsValidation()
        {
            var command = ValidCommand();
            command.Discount = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Discount);
        }

        [Fact]
        public async Task Validate_NegativeOtherCharges_FailsValidation()
        {
            var command = ValidCommand();
            command.OtherCharges = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.OtherCharges);
        }

        [Fact]
        public async Task Validate_InvalidGstNo_FailsValidation()
        {
            var command = ValidCommand();
            command.GstNo = "INVALIDGST";
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GstNo);
        }

        [Fact]
        public async Task Validate_NegativeTCS_FailsValidation()
        {
            var command = ValidCommand();
            command.TCS = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TCS);
        }

        [Fact]
        public async Task Validate_NegativeCess_FailsValidation()
        {
            var command = ValidCommand();
            command.Cess = -1m;
            SetupAllAsyncMocks("IRN001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Cess);
        }
    }
}
