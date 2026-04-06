using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Presentation.Validation.EWaybillHeader;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.EWaybillHeader
{
    public sealed class CreateEWaybillHeaderCommandValidatorTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateEWaybillHeaderCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string? ewbNumber = null)
        {
            if (ewbNumber != null)
                _mockQueryRepo.Setup(r => r.EWBNumberExistsAsync(ewbNumber, null)).ReturnsAsync(false);
        }

        private static CreateEWaybillHeaderCommand ValidCommand() =>
            new()
            {
                UnitId = 1,
                EWBNumber = "EWB001",
                InvoiceNo = "INV001",
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                InvoiceValue = 1000m,
                TotalValue = 1000m,
                CGST = 90m,
                SGST = 90m,
                IGST = 0m,
                Cess = 0m,
                VehicleNo = "MH01AB1234",
                TransDocNo = "TRDOC001"
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_DuplicateEWBNumber_FailsValidation()
        {
            var command = ValidCommand();
            _mockQueryRepo.Setup(r => r.EWBNumberExistsAsync("EWB001", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EWBNumber);
        }

        [Fact]
        public async Task Validate_EWBNumberExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.EWBNumber = new string('A', 51);
            SetupAllAsyncMocks(command.EWBNumber);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EWBNumber);
        }

        [Fact]
        public async Task Validate_InvoiceNoExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.InvoiceNo = new string('A', 31);
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceNo);
        }

        [Fact]
        public async Task Validate_VehicleNoExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.VehicleNo = new string('A', 21);
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.VehicleNo);
        }

        [Fact]
        public async Task Validate_NegativeInvoiceValue_FailsValidation()
        {
            var command = ValidCommand();
            command.InvoiceValue = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InvoiceValue);
        }

        [Fact]
        public async Task Validate_NegativeTotalValue_FailsValidation()
        {
            var command = ValidCommand();
            command.TotalValue = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TotalValue);
        }

        [Fact]
        public async Task Validate_NegativeCGST_FailsValidation()
        {
            var command = ValidCommand();
            command.CGST = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CGST);
        }

        [Fact]
        public async Task Validate_NegativeSGST_FailsValidation()
        {
            var command = ValidCommand();
            command.SGST = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SGST);
        }

        [Fact]
        public async Task Validate_NegativeIGST_FailsValidation()
        {
            var command = ValidCommand();
            command.IGST = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IGST);
        }

        [Fact]
        public async Task Validate_NegativeCess_FailsValidation()
        {
            var command = ValidCommand();
            command.Cess = -1m;
            SetupAllAsyncMocks("EWB001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Cess);
        }
    }
}
