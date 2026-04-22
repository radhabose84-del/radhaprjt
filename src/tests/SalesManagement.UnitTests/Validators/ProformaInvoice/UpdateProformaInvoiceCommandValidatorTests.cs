using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using SalesManagement.Presentation.Validation.ProformaInvoice;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.ProformaInvoice
{
    public sealed class UpdateProformaInvoiceCommandValidatorTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateProformaInvoiceCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateProformaInvoiceCommand ValidCommand(
            int id = 1,
            int? statusId = 1,
            string? remarks = "Updated remarks",
            int isActive = 1)
        {
            return new UpdateProformaInvoiceCommand
            {
                Id = id,
                StatusId = statusId,
                Remarks = remarks,
                IsActive = isActive
            };
        }

        private void SetupAllAsyncMocksPass(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
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

        // ── Id / NotFound Rules ───────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var command = ValidCommand(id: id);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = ValidCommand(id: 99);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── StatusId Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task StatusId_NotFound_FailsFKValidation()
        {
            var command = ValidCommand(statusId: 99);
            SetupAllAsyncMocksPass();
            _mockQueryRepo.Setup(r => r.StatusExistsAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.StatusId);
        }

        [Fact]
        public async Task StatusId_Null_SkipsFKValidation()
        {
            var command = ValidCommand(statusId: null);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.StatusId);
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

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var command = ValidCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = ValidCommand(isActive: isActive);
            SetupAllAsyncMocksPass();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
