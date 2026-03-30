using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Presentation.Validation.Item.ItemCategory;

namespace InventoryManagement.UnitTests.Validators.ItemCategory
{
    public sealed class DeleteItemCategoryCommandValidatorTests
    {
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteItemCategoryCommandValidatorTests()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ItemCategoryDto { Id = 1 });

            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(It.IsAny<int>()))
                .ReturnsAsync(false); // false = no FK dependencies = can delete
        }

        private DeleteItemCategoryCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteItemCategoryCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteItemCategoryCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_HasFKDependencies_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(true); // true = has deps

            var command = new DeleteItemCategoryCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
