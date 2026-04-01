using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemManufacturingDtoValidatorTests
    {
        private ItemManufacturingDtoValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new ItemManufactureDto { UnitId = 1, ManufacturingTypeId = 1 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var dto = new ItemManufactureDto { UnitId = 0, ManufacturingTypeId = 1 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroManufacturingTypeId_FailsValidation()
        {
            var dto = new ItemManufactureDto { UnitId = 1, ManufacturingTypeId = 0 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeUnitId_FailsValidation()
        {
            var dto = new ItemManufactureDto { UnitId = -1, ManufacturingTypeId = 1 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
