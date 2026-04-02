using FluentValidation;
using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class UpdateItemCommandValidatorTests
    {
        private readonly Mock<IItemCommandRepository> _mockItemRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMaxLengthProvider> _mockMaxLen = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemPurchaseDto>> _mockPurchaseV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemInventoryDto>> _mockInventoryV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemQualityDto>> _mockQualityV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemSupplierDto>> _mockSupplierV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemManufactureDto>> _mockManuV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemUomDto>> _mockUomV = new(MockBehavior.Loose);

        public UpdateItemCommandValidatorTests()
        {
            _mockItemRepo
                .Setup(r => r.ExistsByNameSmartForUpdateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockItemRepo
                .Setup(r => r.ExistsByCodeForUpdateAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQryRepo
                .Setup(r => r.GetCandidateItemNamesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
        }

        private UpdateItemCommandValidator CreateValidator() => new(
            _mockItemRepo.Object,
            _mockQryRepo.Object,
            _mockMaxLen.Object,
            _mockPurchaseV.Object,
            _mockInventoryV.Object,
            _mockQualityV.Object,
            _mockSupplierV.Object,
            _mockManuV.Object,
            _mockUomV.Object
        );

        private static UpdateItemCommand ValidCommand() => new()
        {
            Payload = new ItemDto
            {
                Id = 1,
                ItemName = "Updated Item",
                ItemGroupId = 1,
                ItemCategoryId = 1,
                Suppliers = new List<ItemSupplierDto>(),
                Manufacture = new List<ItemManufactureDto>(),
                Uoms = new List<ItemUomDto>(),
                VariantAttributes = new List<VariantAttributeDto>(),
                VariantValues = new List<VariantValueDto>()
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Payload.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyItemName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.Payload.ItemName = name;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateItemName_FailsValidation()
        {
            _mockItemRepo
                .Setup(r => r.ExistsByNameSmartForUpdateAsync("Updated Item", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateSupplierRows_FailsValidation()
        {
            var command = ValidCommand();
            command.Payload.Suppliers = new List<ItemSupplierDto>
            {
                new ItemSupplierDto { SupplierId = 1, UnitId = 1 },
                new ItemSupplierDto { SupplierId = 1, UnitId = 1 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
