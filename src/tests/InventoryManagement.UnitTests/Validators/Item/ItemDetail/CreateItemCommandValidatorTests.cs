using FluentValidation;
using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class CreateItemCommandValidatorTests
    {
        private readonly Mock<IItemCommandRepository> _mockItemRepo = new(MockBehavior.Loose);
        private readonly Mock<IMaxLengthProvider> _mockMaxLen = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemPurchaseDto>> _mockPurchaseV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemInventoryDto>> _mockInventoryV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemQualityDto>> _mockQualityV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemSupplierDto>> _mockSupplierV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemManufactureDto>> _mockManuV = new(MockBehavior.Loose);
        private readonly Mock<IValidator<ItemUomDto>> _mockUomV = new(MockBehavior.Loose);
        private readonly Mock<IItemQueryRepository> _mockQryRepo = new(MockBehavior.Loose);

        public CreateItemCommandValidatorTests()
        {
            _mockItemRepo
                .Setup(r => r.ExistsByNameSmartForCreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQryRepo
                .Setup(r => r.GetCandidateItemNamesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            _mockMaxLen.Setup(m => m.GetMaxLength<It.IsAnyType>(It.IsAny<string>())).Returns((int?)null);
        }

        private CreateItemCommandValidator CreateValidator() => new(
            _mockItemRepo.Object,
            _mockMaxLen.Object,
            _mockPurchaseV.Object,
            _mockInventoryV.Object,
            _mockQualityV.Object,
            _mockSupplierV.Object,
            _mockManuV.Object,
            _mockUomV.Object,
            _mockQryRepo.Object
        );

        private static CreateItemCommand ValidCommand() => new()
        {
            Payload = new ItemDto
            {
                ItemName = "Test Item",
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
                .Setup(r => r.ExistsByNameSmartForCreateAsync("Test Item", It.IsAny<CancellationToken>()))
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

        [Fact]
        public async Task Validate_DuplicateManufactureRows_FailsValidation()
        {
            var command = ValidCommand();
            command.Payload.Manufacture = new List<ItemManufactureDto>
            {
                new ItemManufactureDto { UnitId = 1, ManufacturingTypeId = 1 },
                new ItemManufactureDto { UnitId = 1, ManufacturingTypeId = 1 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
