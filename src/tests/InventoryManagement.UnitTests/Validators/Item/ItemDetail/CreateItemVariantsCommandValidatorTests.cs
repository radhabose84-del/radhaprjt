using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class CreateItemVariantsCommandValidatorTests
    {
        private CreateItemVariantsCommandValidator CreateValidator() => new();

        private static ItemDto ValidVariantsDto() => new()
        {
            ItemName = "Widget Blue",
            ParentItemId = 1,
            VariantValues = new List<VariantValueDto>
            {
                new VariantValueDto { VariantAttributeId = 1, OptionValue = "Blue", Combo = 1 }
            },
            VariantAttributes = new List<VariantAttributeDto>()
        };

        [Fact]
        public async Task Validate_ValidVariants_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidVariantsDto());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NullParentItemId_FailsValidation()
        {
            var dto = ValidVariantsDto();
            dto.ParentItemId = null;

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroParentItemId_FailsValidation()
        {
            var dto = ValidVariantsDto();
            dto.ParentItemId = 0;

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyVariantValues_FailsValidation()
        {
            var dto = ValidVariantsDto();
            dto.VariantValues = new List<VariantValueDto>();

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_VariantValueWithEmptyOptionValue_FailsValidation()
        {
            var dto = ValidVariantsDto();
            dto.VariantValues = new List<VariantValueDto>
            {
                new VariantValueDto { VariantAttributeId = 1, OptionValue = "", Combo = 1 }
            };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
