using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemPurchaseDtoValidatorTests
    {
        private ItemPurchaseDtoValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new ItemPurchaseDto { PurchaseUomId = 1, LeadTimeDays = 10, GrProcessingTimeDays = 5 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullOptionalFields_PassesValidation()
        {
            var dto = new ItemPurchaseDto { PurchaseUomId = null, LeadTimeDays = null, GrProcessingTimeDays = null };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroPurchaseUomId_FailsValidation()
        {
            var dto = new ItemPurchaseDto { PurchaseUomId = 0 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeLeadTimeDays_FailsValidation()
        {
            var dto = new ItemPurchaseDto { LeadTimeDays = -1 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LeadTimeDaysExceedingMax_FailsValidation()
        {
            var dto = new ItemPurchaseDto { LeadTimeDays = 400 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
