using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Presentation.Validation.AssetSubCategories;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetSubCategories
{
    public sealed class CreateAssetSubCategoriesCommandValidatorTests
    {
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);
        private readonly Mock<IAssetSubCategoriesCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateAssetSubCategoriesCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockCommandRepo.Object);

        private void SetupAllAsyncMocks(string subCategoryName = "TestSubCategory")
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(subCategoryName))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetSubCategoriesBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.SubCategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptySubCategoryName_FailsValidation(string? subCategoryName)
        {
            var command = AssetSubCategoriesBuilders.ValidCreateCommand(subCategoryName: subCategoryName!);
            // FluentValidation runs ALL rules in parallel — setup async mocks even in negative tests
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroAssetCategoriesId_FailsValidation()
        {
            var command = AssetSubCategoriesBuilders.ValidCreateCommand(assetCategoriesId: 0);
            SetupAllAsyncMocks(command.SubCategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = AssetSubCategoriesBuilders.ValidCreateCommand(subCategoryName: "ExistingSubCat");
            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("ExistingSubCat"))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
