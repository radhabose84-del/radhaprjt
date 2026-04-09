using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Presentation.Validation.AssetMaster.AssetLocation;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.AssetLocation
{
    public sealed class CreateAssetLocationCommandValidatorTests
    {
        private CreateAssetLocationCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = AssetLocationTestBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = AssetLocationTestBuilders.ValidCreateCommand(unitId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = AssetLocationTestBuilders.ValidCreateCommand(departmentId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroLocationId_FailsValidation()
        {
            var command = AssetLocationTestBuilders.ValidCreateCommand(locationId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
