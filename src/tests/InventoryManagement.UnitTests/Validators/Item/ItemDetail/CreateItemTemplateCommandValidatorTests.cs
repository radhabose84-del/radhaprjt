using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class CreateItemTemplateCommandValidatorTests
    {
        private CreateItemTemplateCommandValidator CreateValidator() => new();

        private static ItemDto ValidTemplateDto() => new()
        {
            ItemName = "Widget Template",
            ItemGroupId = 1,
            ItemCategoryId = 1,
            HasVariants = true,
            VariantAttributes = new List<VariantAttributeDto>
            {
                new VariantAttributeDto { SpecificationMasterId = 1, Order = 1 }
            },
            VariantValues = new List<VariantValueDto>()
        };

        [Fact]
        public async Task Validate_ValidTemplate_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidTemplateDto());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyItemName_FailsValidation(string? name)
        {
            var dto = ValidTemplateDto();
            dto.ItemName = name;

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroItemGroupId_FailsValidation()
        {
            var dto = ValidTemplateDto();
            dto.ItemGroupId = 0;

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_HasVariantsFalse_FailsValidation()
        {
            var dto = ValidTemplateDto();
            dto.HasVariants = false;

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyVariantAttributes_FailsValidation()
        {
            var dto = ValidTemplateDto();
            dto.VariantAttributes = new List<VariantAttributeDto>();

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateAttributeIds_FailsValidation()
        {
            var dto = ValidTemplateDto();
            dto.VariantAttributes = new List<VariantAttributeDto>
            {
                new VariantAttributeDto { SpecificationMasterId = 1, Order = 1 },
                new VariantAttributeDto { SpecificationMasterId = 1, Order = 2 }
            };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
