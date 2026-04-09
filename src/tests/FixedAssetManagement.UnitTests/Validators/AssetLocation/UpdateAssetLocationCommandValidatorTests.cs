using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Presentation.Validation.AssetMaster.AssetLocation;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.AssetLocation
{
    public sealed class UpdateAssetLocationCommandValidatorTests
    {
        private UpdateAssetLocationCommandValidator CreateValidator() => new();

        private static UpdateAssetLocationCommand ValidCommand() =>
            new UpdateAssetLocationCommand
            {
                AssetId = 1,
                UnitId = 1,
                DepartmentId = 2,
                LocationId = 3,
                SubLocationId = 4
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAssetId_FailsValidation()
        {
            var command = ValidCommand();
            command.AssetId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
