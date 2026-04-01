using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemUomDtoValidatorTests
    {
        private ItemUomDtoValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_EmptyDto_PassesValidation()
        {
            var dto = new ItemUomDto { ConversionUOMId = null, ConversionRate = null };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new ItemUomDto { ConversionUOMId = 2, ConversionRate = 1000m };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroConversionUOMId_FailsValidation()
        {
            var dto = new ItemUomDto { ConversionUOMId = 0 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroConversionRate_FailsValidation()
        {
            var dto = new ItemUomDto { ConversionRate = 0m };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeConversionRate_FailsValidation()
        {
            var dto = new ItemUomDto { ConversionRate = -1m };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
