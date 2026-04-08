using FluentValidation.TestHelper;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Application.Purchase.Quotations.Validation;

namespace PurchaseManagement.UnitTests.Validators.Quotation.QuotationEntry
{
    public sealed class QuotationDetailValidatorTests
    {
        private QuotationDetailValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ZeroItemId_FailsValidation()
        {
            var detail = new QuotationDetail { ItemId = 0, UomId = 1, Quantity = 10, Rate = 100 };

            var result = await CreateValidator().TestValidateAsync(detail);

            result.ShouldHaveValidationErrorFor(x => x.ItemId);
        }

        [Fact]
        public async Task Validate_ZeroQuantity_FailsValidation()
        {
            var detail = new QuotationDetail { ItemId = 1, UomId = 1, Quantity = 0, Rate = 100 };

            var result = await CreateValidator().TestValidateAsync(detail);

            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public async Task Validate_ValidDetail_PassesValidation()
        {
            var detail = new QuotationDetail
            {
                ItemId = 1, UomId = 1, Quantity = 10, Rate = 100,
                Discount = 0, GstPercent = 18, Warranty = 12
            };

            var result = await CreateValidator().TestValidateAsync(detail);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
