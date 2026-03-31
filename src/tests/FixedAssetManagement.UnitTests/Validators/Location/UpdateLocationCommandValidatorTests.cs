using FAM.Application.Location.Command.UpdateLocation;
using FAM.Presentation.Validation.Common;
using FAM.Presentation.Validation.Locations;
using FluentValidation.TestHelper;
using FixedAssetManagement.UnitTests.TestData;

namespace FixedAssetManagement.UnitTests.Validators.Location
{
    public sealed class UpdateLocationCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateLocationCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = LocationBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLocationName_FailsValidation(string? locationName)
        {
            var command = LocationBuilders.ValidUpdateCommand(locationName: locationName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
