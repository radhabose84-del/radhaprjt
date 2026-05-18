using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Presentation.Validation.AssetSubCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetSubCategories
{
    public sealed class UpdateAssetSubCategoriesCommandValidatorTests
    {
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        private UpdateAssetSubCategoriesCommandValidator CreateValidator() =>
            new(_maxLengthProvider);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetSubCategoriesBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptySubCategoryName_FailsValidation(string? subCategoryName)
        {
            var command = AssetSubCategoriesBuilders.ValidUpdateCommand(subCategoryName: subCategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetCategoriesId_FailsValidation()
        {
            var command = AssetSubCategoriesBuilders.ValidUpdateCommand(assetCategoriesId: 0);

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
            var command = AssetSubCategoriesBuilders.ValidUpdateCommand(description: description);

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
            var command = AssetSubCategoriesBuilders.ValidUpdateCommand(description: description);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorMessage("Description  must not contain special characters.");
        }
    }
}
