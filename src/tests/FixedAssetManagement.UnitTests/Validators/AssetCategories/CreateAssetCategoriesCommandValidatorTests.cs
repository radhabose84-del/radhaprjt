using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Presentation.Validation.AssetCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetCategories
{
    public sealed class CreateAssetCategoriesCommandValidatorTests
    {
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);
        private readonly Mock<IAssetCategoriesCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateAssetCategoriesCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockCommandRepo.Object);

        private void SetupAllAsyncMocks(string categoryName = "TestCategory")
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(categoryName, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCategoryName_FailsValidation(string? categoryName)
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand(categoryName: categoryName!);
            // FluentValidation runs ALL rules in parallel — setup async mocks even for negative tests
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetGroupId_FailsValidation()
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand(assetGroupId: 0);
            SetupAllAsyncMocks(command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCategoryName_FailsValidation()
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand(categoryName: "ExistingCategory");
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("ExistingCategory", null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        // Description must accept alphabets, numbers and spaces
        [Theory]
        [InlineData("Category 1")]
        [InlineData("Asset Type 2")]
        [InlineData("Plain text description")]
        [InlineData("12345")]
        public async Task Validate_DescriptionWithLettersNumbersSpaces_PassesDescriptionRule(string description)
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand(description: description);
            SetupAllAsyncMocks(command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        // Only special characters are restricted
        [Theory]
        [InlineData("Cat@1")]
        [InlineData("Asset#2")]
        [InlineData("Bad$Desc")]
        public async Task Validate_DescriptionWithSpecialCharacters_FailsDescriptionRule(string description)
        {
            var command = AssetCategoriesBuilders.ValidCreateCommand(description: description);
            SetupAllAsyncMocks(command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  must not contain special characters.");
        }
    }
}
