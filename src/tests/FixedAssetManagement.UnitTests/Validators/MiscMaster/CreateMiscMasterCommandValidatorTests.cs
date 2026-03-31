using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.MiscMaster;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDescription_FailsValidation(string? description)
        {
            var command = MiscMasterBuilders.ValidCreateCommand(description: description!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroMiscTypeId_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidCreateCommand(miscTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
