using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;
using SalesManagement.Presentation.Validation.ProformaInvoice;

namespace SalesManagement.UnitTests.Validators.ProformaInvoice
{
    public sealed class DeleteProformaInvoiceCommandValidatorTests
    {
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteProformaInvoiceCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsDraftStatusAsync(id)).ReturnsAsync(true);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_DraftStatus_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task Id_Zero_FailsNotEmpty()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsDraftStatusAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task Id_EntityNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsDraftStatusAsync(999)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(999));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        // ── Draft Status Rules ────────────────────────────────────────────────

        [Fact]
        public async Task Id_NotDraftStatus_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(2)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsDraftStatusAsync(2)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(2));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Only Proforma Invoices in Draft status can be deleted.");
        }

        [Fact]
        public async Task Id_DraftStatus_PassesDraftCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteProformaInvoiceCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
