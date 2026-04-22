using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Presentation.Validation.ProformaInvoice;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ProformaInvoice
{
    public sealed class CreateProformaInvoiceCommandValidatorTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateProformaInvoiceCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateProformaInvoiceCommand ValidCommand(
            int salesOrderId = 1,
            int partyId = 1,
            decimal proformaAmount = 5000m,
            int? statusId = 1,
            string? remarks = "Test remarks")
        {
            return new CreateProformaInvoiceCommand
            {
                ProformaDate = DateOnly.FromDateTime(DateTime.Today),
                SalesOrderId = salesOrderId,
                PartyId = partyId,
                ProformaAmount = proformaAmount,
                StatusId = statusId,
                Remarks = remarks
            };
        }

        private void SetupAllAsyncMocksPass()
        {
            _mockQueryRepo.Setup(r => r.SalesOrderExistsAndApprovedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesOrderHasAdvancePaymentTypeAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetSalesOrderBalanceAsync(It.IsAny<int>())).ReturnsAsync(10000m);
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

        // ── SalesOrderId Rules ────────────────────────────────────────────────

        [Fact]
        public async Task SalesOrderId_Zero_FailsNotEmpty()
        {
            var command = ValidCommand(salesOrderId: 0);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrderId);
        }

        [Fact]
        public async Task SalesOrderId_NotApproved_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.SalesOrderExistsAndApprovedAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrderId)
                  .WithErrorMessage("Sales Order does not exist or is not in Approved status.");
        }

        [Fact]
        public async Task SalesOrderId_NotAdvancePaymentType_FailsValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.SalesOrderHasAdvancePaymentTypeAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrderId)
                  .WithErrorMessage("Sales Order does not have Advance payment type. Proforma Invoice can only be generated for Advance payment Sales Orders.");
        }

        // ── PartyId Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task PartyId_Zero_FailsNotEmpty()
        {
            var command = ValidCommand(partyId: 0);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        // ── StatusId Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task StatusId_Null_FailsNotEmpty()
        {
            var command = ValidCommand(statusId: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StatusId);
        }

        [Fact]
        public async Task StatusId_NotFound_FailsFKValidation()
        {
            var command = ValidCommand(statusId: 99);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StatusId);
        }

        // ── ProformaAmount Rules ──────────────────────────────────────────────

        [Fact]
        public async Task ProformaAmount_Zero_FailsGreaterThan()
        {
            var command = ValidCommand(proformaAmount: 0);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ProformaAmount);
        }

        [Fact]
        public async Task ProformaAmount_Negative_FailsGreaterThan()
        {
            var command = ValidCommand(proformaAmount: -100);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ProformaAmount);
        }

        [Fact]
        public async Task ProformaAmount_ExceedsBalance_FailsValidation()
        {
            var command = ValidCommand(proformaAmount: 15000m);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.GetSalesOrderBalanceAsync(1)).ReturnsAsync(10000m);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }

        // ── Remarks Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task Remarks_TooLong_FailsMaxLength()
        {
            var longRemarks = new string('A', 501);
            var command = ValidCommand(remarks: longRemarks);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Remarks);
        }

        [Fact]
        public async Task Remarks_Null_PassesValidation()
        {
            var command = ValidCommand(remarks: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Remarks);
        }
    }
}
