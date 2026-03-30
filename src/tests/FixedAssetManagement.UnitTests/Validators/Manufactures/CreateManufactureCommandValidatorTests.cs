using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.Manufacture;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.Manufactures
{
    public sealed class CreateManufactureCommandValidatorTests
    {
        private CreateManufactureCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ManufacturesBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyManufactureName_FailsValidation(string? name)
        {
            var command = ManufacturesBuilders.ValidCreateCommand(manufactureName: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = ManufacturesBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroCountryId_FailsValidation()
        {
            var command = ManufacturesBuilders.ValidCreateCommand(countryId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroCityId_FailsValidation()
        {
            var command = ManufacturesBuilders.ValidCreateCommand(cityId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
