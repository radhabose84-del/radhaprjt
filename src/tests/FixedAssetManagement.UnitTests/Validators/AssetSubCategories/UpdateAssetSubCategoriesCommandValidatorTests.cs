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
    }
}
