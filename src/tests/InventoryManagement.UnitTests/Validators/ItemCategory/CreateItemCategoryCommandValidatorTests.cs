using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemCategory;

namespace InventoryManagement.UnitTests.Validators.ItemCategory
{
    public sealed class CreateItemCategoryCommandValidatorTests
    {
        private readonly Mock<IMaxLengthProvider> _mockMaxLengthProvider = new(MockBehavior.Loose);
        private readonly Mock<IItemCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IModuleLookup> _mockModuleLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public CreateItemCategoryCommandValidatorTests()
        {
            _mockMaxLengthProvider.Setup(m => m.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>(It.IsAny<string>()))
                .Returns(100);

            _mockCommandRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockModuleLookup.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(new List<ModuleLookupDto>
                {
                    new ModuleLookupDto { ModuleId = 1, ModuleName = "Module1" },
                    new ModuleLookupDto { ModuleId = 2, ModuleName = "Module2" }
                });

            _mockUnitLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new UnitLookupDto { UnitId = id, UnitName = $"Unit-{id}" }).ToList());

            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new UOMLookupDto { Id = id, UOMName = $"UOM-{id}" }).ToList());
        }

        private CreateItemCategoryCommandValidator CreateValidator() =>
            new(_mockMaxLengthProvider.Object, _mockCommandRepo.Object, _mockModuleLookup.Object, _mockUnitLookup.Object, _mockUomLookup.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new CreateItemCategoryCommand { ItemCategoryName = "Electronics", ItemGroupId = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new CreateItemCategoryCommand { ItemCategoryName = name, ItemGroupId = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroGroupId_FailsValidation()
        {
            var command = new CreateItemCategoryCommand { ItemCategoryName = "Electronics", ItemGroupId = 0, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockCommandRepo.Setup(r => r.ExistsByNameAsync("Existing Category")).ReturnsAsync(true);

            var command = new CreateItemCategoryCommand { ItemCategoryName = "Existing Category", ItemGroupId = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
