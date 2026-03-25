using FAM.Application.UOM.Command.CreateUOM;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.UOM;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.UOM
{
    public sealed class CreateUOMCommandValidatorTests
    {
        private CreateUOMCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = FAMUOMBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = FAMUOMBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyUOMName_FailsValidation(string? uomName)
        {
            var command = FAMUOMBuilders.ValidCreateCommand(uomName: uomName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroSortOrder_FailsValidation()
        {
            var command = FAMUOMBuilders.ValidCreateCommand(sortOrder: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroUOMTypeId_FailsValidation()
        {
            var command = FAMUOMBuilders.ValidCreateCommand(uomTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
