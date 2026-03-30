using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Presentation.Validation.Item.ItemGroup;

namespace InventoryManagement.UnitTests.Validators.ItemGroup
{
    public sealed class DeleteItemGroupCommandValidatorTests
    {
        private readonly Mock<IItemGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteItemGroupCommandValidatorTests()
        {
            // Happy path: record exists, no FK dependencies
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new ItemGroupDto { Id = 1 });
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(It.IsAny<int>()))
                .ReturnsAsync(false);
        }

        private DeleteItemGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteItemGroupCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteItemGroupCommand { Id = 0 });
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_HasFKDependencies_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteItemGroupCommand { Id = 1 });
            result.Errors.Should().NotBeEmpty();
        }
    }
}
