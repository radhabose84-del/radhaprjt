using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemInventoryDtoValidatorTests
    {
        private readonly Mock<IMaxLengthProvider> _mockMaxLen = new(MockBehavior.Loose);

        public ItemInventoryDtoValidatorTests()
        {
            _mockMaxLen.Setup(m => m.GetMaxLength<It.IsAnyType>(It.IsAny<string>())).Returns((int?)null);
        }

        private ItemInventoryDtoValidator CreateValidator() => new(_mockMaxLen.Object);

        [Fact]
        public async Task Validate_EmptyDto_PassesValidation()
        {
            var dto = new ItemInventoryDto();
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new ItemInventoryDto
            {
                Weight = 10m,
                WeightUomId = 1,
                ShelfLife = 365,
                UpperTolerance = 5m,
                LowerTolerance = 2m,
                ReorderLevel = 100,
                ReorderQty = 50
            };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NegativeWeight_FailsValidation()
        {
            var dto = new ItemInventoryDto { Weight = -1m };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroWeightUomId_FailsValidation()
        {
            var dto = new ItemInventoryDto { WeightUomId = 0 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ShelfLifeExceedingMax_FailsValidation()
        {
            var dto = new ItemInventoryDto { ShelfLife = 4000 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ApplyBatchNumberWithoutBatchManagement_FailsValidation()
        {
            var dto = new ItemInventoryDto { ApplyBatchNumber = true, BatchManagement = false };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ApplyBatchNumberWithBatchManagement_PassesValidation()
        {
            var dto = new ItemInventoryDto { ApplyBatchNumber = true, BatchManagement = true };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
