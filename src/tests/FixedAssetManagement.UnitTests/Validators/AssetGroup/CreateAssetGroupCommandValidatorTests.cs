using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Presentation.Validation.AssetGroup;
using FAM.Presentation.Validation.Common;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetGroup
{
    public sealed class CreateAssetGroupCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateAssetGroupCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetGroupBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = AssetGroupBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyGroupName_FailsValidation(string? groupName)
        {
            var command = AssetGroupBuilders.ValidCreateCommand(groupName: groupName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("Asset Group One")]
        [InlineData("AssetGroup1")]
        [InlineData("Group 1 2 3")]
        [InlineData("Asset Group 123")]
        public async Task Validate_GroupName_AllowsLettersDigitsAndSpaces(string groupName)
        {
            var command = AssetGroupBuilders.ValidCreateCommand(groupName: groupName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.GroupName);
        }

        [Theory]
        [InlineData("Asset@Group")]
        [InlineData("Asset#Group")]
        [InlineData("Asset$Group")]
        [InlineData("Asset-Group")]
        [InlineData("Asset_Group")]
        [InlineData("Asset!Group")]
        public async Task Validate_GroupName_RejectsSpecialCharacters(string groupName)
        {
            var command = AssetGroupBuilders.ValidCreateCommand(groupName: groupName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GroupName)
                  .WithErrorMessage("GroupName  must not contain special characters.");
        }

        [Theory]
        [InlineData("AG 001")]
        [InlineData("AG-001")]
        [InlineData("AG@001")]
        public async Task Validate_Code_RejectsSpacesAndSpecialCharacters(string code)
        {
            var command = AssetGroupBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Code);
        }
    }
}
