using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemSupplierDtoValidatorTests
    {
        private readonly Mock<IMaxLengthProvider> _mockMaxLen = new(MockBehavior.Loose);

        public ItemSupplierDtoValidatorTests()
        {
            _mockMaxLen.Setup(m => m.GetMaxLength<It.IsAnyType>(It.IsAny<string>())).Returns((int?)null);
        }

        private ItemSupplierDtoValidator CreateValidator() => new(_mockMaxLen.Object);

        [Fact]
        public async Task Validate_ValidDto_PassesValidation()
        {
            var dto = new ItemSupplierDto { SupplierId = 1, UnitId = 1, SupplierPartNo = "PART-001" };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroSupplierId_FailsValidation()
        {
            var dto = new ItemSupplierDto { SupplierId = 0, UnitId = 1 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullSupplierPartNo_PassesValidation()
        {
            var dto = new ItemSupplierDto { SupplierId = 1, UnitId = 1, SupplierPartNo = null };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
