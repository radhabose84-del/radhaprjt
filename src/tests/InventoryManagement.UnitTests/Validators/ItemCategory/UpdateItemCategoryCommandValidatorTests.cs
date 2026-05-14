using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemCategory;

namespace InventoryManagement.UnitTests.Validators.ItemCategory
{
    public sealed class UpdateItemCategoryCommandValidatorTests
    {
        private readonly Mock<IMaxLengthProvider> _mockMaxLengthProvider = new(MockBehavior.Loose);
        private readonly Mock<IItemCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IModuleLookup> _mockModuleLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        public UpdateItemCategoryCommandValidatorTests()
        {
            _mockMaxLengthProvider.Setup(m => m.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemCategory>(It.IsAny<string>()))
                .Returns(100);

            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ItemCategoryDto { Id = 1 });

            _mockQueryRepo.Setup(r => r.IsLinkedWithActiveItemsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo.Setup(r => r.GetSampleQuantitiesAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<SampleQuantityDto>());

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

        private UpdateItemCategoryCommandValidator CreateValidator() =>
            new(_mockMaxLengthProvider.Object, _mockCommandRepo.Object, _mockQueryRepo.Object, _mockModuleLookup.Object, _mockUnitLookup.Object, _mockUomLookup.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new UpdateItemCategoryCommand { Id = 1, ItemCategoryName = "Updated Cat", ItemGroupId = 1, IsActive = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = new UpdateItemCategoryCommand { Id = 1, ItemCategoryName = name, ItemGroupId = 1, IsActive = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            var command = new UpdateItemCategoryCommand { Id = 1, ItemCategoryName = "Existing Name", ItemGroupId = 1, IsActive = 1, ModuleIds = new List<int> { 1 } };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
