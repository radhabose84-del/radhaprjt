using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.BillEntry
{
    public sealed class PurchaseBillEntryCommandValidatorTests
    {
        private PurchaseBillEntryCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ZeroItemId_FailsValidation()
        {
            var dto = new PurchaseBillEntryDetailDto { ItemId = 0, BilledQty = 10, BilledRate = 100 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.ItemId);
        }

        [Fact]
        public async Task Validate_ZeroBilledQty_FailsValidation()
        {
            var dto = new PurchaseBillEntryDetailDto { ItemId = 1, BilledQty = 0, BilledRate = 100 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.BilledQty);
        }

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new PurchaseBillEntryDetailDto { ItemId = 1, BilledQty = 10, BilledRate = 100, TaxPercentage = 5 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
