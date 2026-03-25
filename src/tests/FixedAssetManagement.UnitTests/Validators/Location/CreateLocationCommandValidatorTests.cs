using FAM.Application.Location.Command.CreateLocation;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.Locations;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.Location
{
    public sealed class CreateLocationCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateLocationCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = LocationBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLocationName_FailsValidation(string? locationName)
        {
            var command = LocationBuilders.ValidCreateCommand(locationName: locationName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = LocationBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
