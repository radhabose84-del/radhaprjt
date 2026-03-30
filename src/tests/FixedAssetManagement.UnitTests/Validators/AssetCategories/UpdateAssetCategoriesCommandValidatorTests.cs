using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Presentation.Validation.AssetCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetCategories
{
    public sealed class UpdateAssetCategoriesCommandValidatorTests
    {
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);
        private readonly Mock<IAssetCategoriesCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private UpdateAssetCategoriesCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockCommandRepo.Object);

        private void SetupAllAsyncMocks(string categoryName = "UpdatedCategory", int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(categoryName, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetCategoriesBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.CategoryName!, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCategoryName_FailsValidation(string? categoryName)
        {
            var command = AssetCategoriesBuilders.ValidUpdateCommand(categoryName: categoryName!);
            // FluentValidation runs ALL rules — setup async mocks even in negative tests
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), command.Id))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetGroupId_FailsValidation()
        {
            var command = AssetCategoriesBuilders.ValidUpdateCommand(assetGroupId: 0);
            SetupAllAsyncMocks(command.CategoryName!, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
