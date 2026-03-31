using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.MiscTypeMaster;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(miscTypeCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscTypeMasterBuilders.ValidCreateCommand(description: description!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
