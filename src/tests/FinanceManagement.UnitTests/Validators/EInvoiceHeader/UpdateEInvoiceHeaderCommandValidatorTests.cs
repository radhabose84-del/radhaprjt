using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Presentation.Validation.EInvoiceHeader;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.EInvoiceHeader
{
    public sealed class UpdateEInvoiceHeaderCommandValidatorTests
    {
        private readonly Mock<IEInvoiceHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateEInvoiceHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, string? irnNumber = "IRN001")
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            if (irnNumber != null)
                _mockQueryRepo.Setup(r => r.IrnNumberExistsAsync(irnNumber, id)).ReturnsAsync(false);
        }

        private static UpdateEInvoiceHeaderCommand ValidCommand() =>
            new()
            {
                Id = 1,
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
                OtherCharges = 0m,
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IrnNumberExistsAsync("IRN001", 1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_ZeroPartyId_FailsValidation()
        {
            var command = ValidCommand();
            command.PartyId = 0;
            SetupAllAsyncMocks();

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
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceNo);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = ValidCommand();
            command.IsActive = 5;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_DuplicateIrnNumber_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.IrnNumberExistsAsync("IRN001", 1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IrnNumber);
        }

        [Fact]
        public async Task Validate_NegativeCGST_FailsValidation()
        {
            var command = ValidCommand();
            command.CGST = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CGST);
        }

        [Fact]
        public async Task Validate_NegativeDiscount_FailsValidation()
        {
            var command = ValidCommand();
            command.Discount = -1m;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Discount);
        }

        [Fact]
        public async Task Validate_InvalidGstNo_FailsValidation()
        {
            var command = ValidCommand();
            command.GstNo = "BADGST";
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GstNo);
        }
    }
}
